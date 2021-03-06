﻿/*
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

	using IncorrectUsage = Org.Neo4j.Commandline.admin.IncorrectUsage;
	using OutsideWorld = Org.Neo4j.Commandline.admin.OutsideWorld;
	using Args = Org.Neo4j.Helpers.Args;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Converters = Org.Neo4j.Kernel.impl.util.Converters;
	using Validators = Org.Neo4j.Kernel.impl.util.Validators;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.database_path;

	internal class DatabaseImporter : Importer
	{
		 private readonly File _from;
		 private readonly Config _config;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: DatabaseImporter(org.neo4j.helpers.Args args, org.neo4j.kernel.configuration.Config config, org.neo4j.commandline.admin.OutsideWorld outsideWorld) throws org.neo4j.commandline.admin.IncorrectUsage
		 internal DatabaseImporter( Args args, Config config, OutsideWorld outsideWorld )
		 {
			  this._config = config;

			  try
			  {
					this._from = args.InterpretOption( "from", Converters.mandatory(), Converters.toFile(), Validators.CONTAINS_EXISTING_DATABASE );
			  }
			  catch ( System.ArgumentException e )
			  {
					throw new IncorrectUsage( e.Message );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doImport() throws java.io.IOException
		 public override void DoImport()
		 {
			  CopyDatabase( _from, _config );
			  RemoveMessagesLog( _config );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void copyDatabase(java.io.File from, org.neo4j.kernel.configuration.Config config) throws java.io.IOException
		 private void CopyDatabase( File from, Config config )
		 {
			  FileUtils.copyRecursively( from, config.Get( database_path ) );
		 }

		 private void RemoveMessagesLog( Config config )
		 {
			  FileUtils.deleteFile( new File( config.Get( database_path ), "messages.log" ) );
		 }
	}

}