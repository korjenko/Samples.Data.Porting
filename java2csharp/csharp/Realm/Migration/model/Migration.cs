﻿/*
 * Copyright 2014 Realm Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace io.realm.examples.realmmigrationexample.model
{

	using ColumnType = io.realm.@internal.ColumnType;
	using Table = io.realm.@internal.Table;

	/// <summary>
	///*************************** NOTE: *********************************************
	/// The API for migration is currently using internal lower level classes that will
	/// be replaced by a new API very soon! Until then you will have to explore and use
	/// below example as inspiration.
	/// *********************************************************************************
	/// </summary>


	public class Migration : RealmMigration
	{
		public override long execute(Realm realm, long version)
		{

			/*
			    // Version 0
			    class Person
			        @Required
			        String firstName;
			        @Required
			        String lastName;
			        int    age;
	
			    // Version 1
			    class Person
			        @Required
			        String fullName;        // combine firstName and lastName into single field.
			        int age;
			*/

			// Migrate from version 0 to version 1
			if (version == 0)
			{
				Table personTable = realm.getTable(typeof(Person));

				long fistNameIndex = getIndexForProperty(personTable, "firstName");
				long lastNameIndex = getIndexForProperty(personTable, "lastName");
				long fullNameIndex = personTable.addColumn(ColumnType.STRING, "fullName");
				for (int i = 0; i < personTable.size(); i++)
				{
					personTable.setString(fullNameIndex, i, personTable.getString(fistNameIndex, i) + " " + personTable.getString(lastNameIndex, i));
				}
				personTable.removeColumn(getIndexForProperty(personTable, "firstName"));
				personTable.removeColumn(getIndexForProperty(personTable, "lastName"));
				version++;
			}

			/*
			    // Version 2
			        class Pet                   // add a new model class
			            @Required
			            String name;
			            @Required
			            String type;
	
			        class Person
			            @Required
			            String fullName;
			            int age;
			            RealmList<Pet> pets;    // add an array property
	
			*/
			// Migrate from version 1 to version 2
			if (version == 1)
			{
				Table personTable = realm.getTable(typeof(Person));
				Table petTable = realm.getTable(typeof(Pet));
				long nameColumnIndex = petTable.addColumn(ColumnType.STRING, "name");
				long typeColumnIndex = petTable.addColumn(ColumnType.STRING, "type");
				long petsIndex = personTable.addColumnLink(ColumnType.LINK_LIST, "pets", petTable);
				long fullNameIndex = getIndexForProperty(personTable, "fullName");

				for (int i = 0; i < personTable.size(); i++)
				{
					if (personTable.getString(fullNameIndex, i).Equals("JP McDonald"))
					{
						long rowIndex = petTable.addEmptyRow();
						petTable.setString(nameColumnIndex, rowIndex, "Jimbo");
						petTable.setString(typeColumnIndex, rowIndex, "dog");
						personTable.getUncheckedRow(i).getLinkList(petsIndex).add(rowIndex);
					}
				}
				version++;
			}

			/*
			    // Version 3
			        class Pet
			            @Required
			            String name;
			            int type;               // type becomes int
	
			        class Person
			            String fullName;        // fullName is nullable now
			            RealmList<Pet> pets;    // age and pets re-ordered
			            int age;
			*/
			// Migrate from version 2 to version 3
			if (version == 2)
			{
				Table personTable = realm.getTable(typeof(Person));
				long fullNameIndex = getIndexForProperty(personTable, "fullName");
				// fullName is nullable now.
				personTable.convertColumnToNullable(fullNameIndex);

				Table petTable = realm.getTable(typeof(Pet));
				long oldTypeIndex = getIndexForProperty(petTable, "type");
				long typeIndex = petTable.addColumn(ColumnType.INTEGER, "type");
				for (int i = 0; i < petTable.size(); i++)
				{
					string type = petTable.getString(oldTypeIndex, i);
					if (type.Equals("dog"))
					{
						petTable.setLong(typeIndex, i, 1);
					}
					else if (type.Equals("cat"))
					{
						petTable.setLong(typeIndex, i, 2);
					}
					else if (type.Equals("hamster"))
					{
						petTable.setLong(typeIndex, i, 3);
					}
				}
				petTable.removeColumn(oldTypeIndex);
				version++;
			}
			return version;
		}

		private long getIndexForProperty(Table table, string name)
		{
			for (int i = 0; i < table.ColumnCount; i++)
			{
				if (table.getColumnName(i).Equals(name))
				{
					return i;
				}
			}
			return -1;
		}
	}

}