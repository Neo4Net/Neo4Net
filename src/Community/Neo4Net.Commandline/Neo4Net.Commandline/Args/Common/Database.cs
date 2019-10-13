﻿/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.CommandLine.Args.Common
{

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Args = Neo4Net.Helpers.Args;

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