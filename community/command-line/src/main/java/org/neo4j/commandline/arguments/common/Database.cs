/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Commandline.arguments.common
{

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Args = Org.Neo4j.Helpers.Args;

	public class Database : OptionalNamedArg
	{
		 public const string ARG_DATABASE = "database";

		 public Database() : this("Name of database.")
		 {
		 }

		 public Database( string description ) : base( ARG_DATABASE, "name", GraphDatabaseSettings.DEFAULT_DATABASE_NAME, description )
		 {
		 }

		 private static string Validate( string dbName )
		 {
			  if ( dbName.Contains( File.separator ) )
			  {
					throw new System.ArgumentException( "'database' should be a name but you seem to have specified a path: " + dbName );
			  }
			  return dbName;
		 }

		 public override string Parse( Args parsedArgs )
		 {
			  return Validate( base.Parse( parsedArgs ) );
		 }
	}

}