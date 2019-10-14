/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.backup.impl
{
	using ConsistencyCheckService = Neo4Net.Consistency.ConsistencyCheckService;
	using ConsistencyCheckIncompleteException = Neo4Net.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using ConsistencyFlags = Neo4Net.Consistency.checking.full.ConsistencyFlags;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public interface ConsistencyCheck
	{
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 ConsistencyCheck NONE = new ConsistencyCheck()
	//				{
	//					 @@Override public String name()
	//					 {
	//						  return "none";
	//					 }
	//
	//					 @@Override public boolean runFull(DatabaseLayout databaseLayout, Config tuningConfiguration, ProgressMonitorFactory progressFactory, LogProvider logProvider, FileSystemAbstraction fileSystem, PageCache pageCache, boolean verbose, ConsistencyFlags consistencyFlags)
	//					 {
	//						  return true;
	//					 }
	//				};

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 ConsistencyCheck FULL = new ConsistencyCheck()
	//				{
	//					 @@Override public String name()
	//					 {
	//						  return "full";
	//					 }
	//
	//					 @@Override public boolean runFull(DatabaseLayout databaseLayout, Config tuningConfiguration, ProgressMonitorFactory progressFactory, LogProvider logProvider, FileSystemAbstraction fileSystem, PageCache pageCache, boolean verbose, ConsistencyFlags consistencyFlags) throws ConsistencyCheckFailedException
	//					 {
	//						  try
	//						  {
	//								return new ConsistencyCheckService().runFullConsistencyCheck(databaseLayout, tuningConfiguration, progressFactory, logProvider, fileSystem, pageCache, verbose, consistencyFlags).isSuccessful();
	//						  }
	//						  catch (ConsistencyCheckIncompleteException e)
	//						  {
	//								throw new ConsistencyCheckFailedException(e);
	//						  }
	//					 }
	//				};

		 string Name();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean runFull(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.kernel.configuration.Config tuningConfiguration, org.neo4j.helpers.progress.ProgressMonitorFactory progressFactory, org.neo4j.logging.LogProvider logProvider, org.neo4j.io.fs.FileSystemAbstraction fileSystem, org.neo4j.io.pagecache.PageCache pageCache, boolean verbose, org.neo4j.consistency.checking.full.ConsistencyFlags consistencyFlags) throws ConsistencyCheckFailedException;
		 bool RunFull( DatabaseLayout databaseLayout, Config tuningConfiguration, ProgressMonitorFactory progressFactory, LogProvider logProvider, FileSystemAbstraction fileSystem, PageCache pageCache, bool verbose, ConsistencyFlags consistencyFlags );

		 String ();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static ConsistencyCheck fromString(String name)
	//	 {
	//		  for (ConsistencyCheck consistencyCheck : new ConsistencyCheck[]{NONE, FULL})
	//		  {
	//				if (consistencyCheck.name().equalsIgnoreCase(name))
	//				{
	//					 return consistencyCheck;
	//				}
	//		  }
	//		  throw new IllegalArgumentException("Unknown consistency check name: " + name + ". Supported values: NONE, FULL");
	//	 }
	}

}