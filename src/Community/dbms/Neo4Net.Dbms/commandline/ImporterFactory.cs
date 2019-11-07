/*
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
namespace Neo4Net.Dbms.CommandLine
{
	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using Args = Neo4Net.Helpers.Args;
	using Config = Neo4Net.Kernel.configuration.Config;

	internal class ImporterFactory
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Importer getImporterForMode(String mode, Neo4Net.helpers.Args parsedArgs, Neo4Net.kernel.configuration.Config config, Neo4Net.commandline.admin.OutsideWorld outsideWorld) throws Neo4Net.commandline.admin.IncorrectUsage, Neo4Net.commandline.admin.CommandFailed
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