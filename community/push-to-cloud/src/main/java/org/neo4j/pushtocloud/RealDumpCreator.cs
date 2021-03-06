﻿/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Org.Neo4j.Pushtocloud
{

	using CommandFailed = Org.Neo4j.Commandline.admin.CommandFailed;
	using IncorrectUsage = Org.Neo4j.Commandline.admin.IncorrectUsage;
	using OutsideWorld = Org.Neo4j.Commandline.admin.OutsideWorld;
	using DumpCommandProvider = Org.Neo4j.Commandline.dbms.DumpCommandProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.array;

	internal class RealDumpCreator : PushToCloudCommand.DumpCreator
	{
		 private readonly Path _homeDir;
		 private readonly Path _configDir;
		 private readonly OutsideWorld _outsideWorld;

		 internal RealDumpCreator( Path homeDir, Path configDir, OutsideWorld outsideWorld )
		 {
			  this._homeDir = homeDir;
			  this._configDir = configDir;
			  this._outsideWorld = outsideWorld;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void dumpDatabase(String database, java.nio.file.Path targetDumpFile) throws org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage
		 public override void DumpDatabase( string database, Path targetDumpFile )
		 {
			  string[] args = array( "--database", database, "--to", targetDumpFile.ToString() );
			  ( new DumpCommandProvider() ).create(_homeDir, _configDir, _outsideWorld).execute(args);
			  _outsideWorld.outStream().printf("Dumped contents of database '%s' into '%s'%n", database, targetDumpFile);
		 }
	}

}