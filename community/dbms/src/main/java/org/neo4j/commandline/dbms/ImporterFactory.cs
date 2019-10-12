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
namespace Org.Neo4j.Commandline.dbms
{
	using CommandFailed = Org.Neo4j.Commandline.admin.CommandFailed;
	using IncorrectUsage = Org.Neo4j.Commandline.admin.IncorrectUsage;
	using OutsideWorld = Org.Neo4j.Commandline.admin.OutsideWorld;
	using Args = Org.Neo4j.Helpers.Args;
	using Config = Org.Neo4j.Kernel.configuration.Config;

	internal class ImporterFactory
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Importer getImporterForMode(String mode, org.neo4j.helpers.Args parsedArgs, org.neo4j.kernel.configuration.Config config, org.neo4j.commandline.admin.OutsideWorld outsideWorld) throws org.neo4j.commandline.admin.IncorrectUsage, org.neo4j.commandline.admin.CommandFailed
		 internal virtual Importer GetImporterForMode( string mode, Args parsedArgs, Config config, OutsideWorld outsideWorld )
		 {
			  Importer importer;
			  switch ( mode )
			  {
			  case "database":
					importer = new DatabaseImporter( parsedArgs, config, outsideWorld );
					break;
			  case "csv":
					importer = new CsvImporter( parsedArgs, config, outsideWorld );
					break;
			  default:
					throw new CommandFailed( "Invalid mode specified." ); // This won't happen because mode is mandatory.
			  }
			  return importer;
		 }
	}

}