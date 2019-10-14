using System.Collections.Generic;

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
namespace Neo4Net.Kernel.ha
{

	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.storecopy.StoreUtil.cleanStoreDir;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.storecopy.StoreUtil.deleteRecursive;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.storecopy.StoreUtil.getBranchedDataRootDirectory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.storecopy.StoreUtil.isBranchedDataDirectory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.storecopy.StoreUtil.moveAwayDb;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.storecopy.StoreUtil.newBranchedDataDir;

	public abstract class BranchedDataPolicy
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       keep_all { public void handle(java.io.File databaseDirectory, org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.logging.internal.LogService logService) throws java.io.IOException { org.neo4j.logging.Log msgLog = logService.getInternalLog(getClass()); java.io.File branchedDataDir = newBranchedDataDir(databaseDirectory); msgLog.debug("Moving store from " + databaseDirectory + " to " + branchedDataDir); moveAwayDb(databaseDirectory, branchedDataDir); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       keep_last { public void handle(java.io.File databaseDirectory, org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.logging.internal.LogService logService) throws java.io.IOException { org.neo4j.logging.Log msgLog = logService.getInternalLog(getClass()); java.io.File branchedDataDir = newBranchedDataDir(databaseDirectory); msgLog.debug("Moving store from " + databaseDirectory + " to " + branchedDataDir); moveAwayDb(databaseDirectory, branchedDataDir); File[] files = getBranchedDataRootDirectory(databaseDirectory).listFiles(); if(files != null) { for(java.io.File file : files) { if(isBranchedDataDirectory(file) && !file.equals(branchedDataDir)) { deleteRecursive(file); } } } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       keep_none { public void handle(java.io.File databaseDirectory, org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.logging.internal.LogService logService) throws java.io.IOException { org.neo4j.logging.Log msgLog = logService.getInternalLog(getClass()); msgLog.debug("Removing store  " + databaseDirectory); cleanStoreDir(databaseDirectory); } };

		 private static readonly IList<BranchedDataPolicy> valueList = new List<BranchedDataPolicy>();

		 static BranchedDataPolicy()
		 {
			 valueList.Add( keep_all );
			 valueList.Add( keep_last );
			 valueList.Add( keep_none );
		 }

		 public enum InnerEnum
		 {
			 keep_all,
			 keep_last,
			 keep_none
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private BranchedDataPolicy( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void handle(java.io.File databaseDirectory, org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.logging.internal.LogService msgLog) throws java.io.IOException;
		 public abstract void handle( java.io.File databaseDirectory, Neo4Net.Io.pagecache.PageCache pageCache, Neo4Net.Logging.Internal.LogService msgLog );

		public static IList<BranchedDataPolicy> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static BranchedDataPolicy valueOf( string name )
		{
			foreach ( BranchedDataPolicy enumInstance in BranchedDataPolicy.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}