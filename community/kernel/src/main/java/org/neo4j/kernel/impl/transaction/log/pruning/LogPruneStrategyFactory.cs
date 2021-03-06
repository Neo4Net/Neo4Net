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
namespace Org.Neo4j.Kernel.impl.transaction.log.pruning
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using ThresholdConfigValue = Org.Neo4j.Kernel.impl.transaction.log.pruning.ThresholdConfigParser.ThresholdConfigValue;
	using VisibleForTesting = Org.Neo4j.Util.VisibleForTesting;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.pruning.ThresholdConfigParser.parse;

	public class LogPruneStrategyFactory
	{
		 private static readonly LogPruneStrategy NO_PRUNING = new LogPruneStrategyAnonymousInnerClass();

		 private class LogPruneStrategyAnonymousInnerClass : LogPruneStrategy
		 {
			 public LongStream findLogVersionsToDelete( long upToLogVersion )
			 {
				  // Never delete anything.
				  return LongStream.empty();
			 }

			 public override string ToString()
			 {
				  return "NO_PRUNING";
			 }
		 }

		 public LogPruneStrategyFactory()
		 {
		 }

		 /// <summary>
		 /// Parses a configuration value for log specifying log pruning. It has one of these forms:
		 /// <ul>
		 ///   <li>all</li>
		 ///   <li>[number][unit] [type]</li>
		 /// </ul>
		 /// For example:
		 /// <ul>
		 ///   <li>100M size - For keeping last 100 megabytes of log data</li>
		 ///   <li>20 pcs - For keeping last 20 non-empty log files</li>
		 ///   <li>7 days - For keeping last 7 days worth of log data</li>
		 ///   <li>1k hours - For keeping last 1000 hours worth of log data</li>
		 /// </ul>
		 /// </summary>
		 internal virtual LogPruneStrategy StrategyFromConfigValue( FileSystemAbstraction fileSystem, LogFiles logFiles, Clock clock, string configValue )
		 {
			  ThresholdConfigValue value = parse( configValue );

			  if ( value == ThresholdConfigValue.NO_PRUNING )
			  {
					return NO_PRUNING;
			  }

			  Threshold thresholdToUse = GetThresholdByType( fileSystem, clock, value, configValue );
			  return new ThresholdBasedPruneStrategy( fileSystem, logFiles, thresholdToUse );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting static Threshold getThresholdByType(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.time.Clock clock, org.neo4j.kernel.impl.transaction.log.pruning.ThresholdConfigParser.ThresholdConfigValue value, String originalConfigValue)
		 internal static Threshold GetThresholdByType( FileSystemAbstraction fileSystem, Clock clock, ThresholdConfigValue value, string originalConfigValue )
		 {
			  long thresholdValue = value.Value;

			  switch ( value.Type )
			  {
					case "files":
						 return new FileCountThreshold( thresholdValue );
					case "size":
						 return new FileSizeThreshold( fileSystem, thresholdValue );
					case "txs":
					case "entries": // txs and entries are synonyms
						 return new EntryCountThreshold( thresholdValue );
					case "hours":
						 return new EntryTimespanThreshold( clock, HOURS, thresholdValue );
					case "days":
						 return new EntryTimespanThreshold( clock, DAYS, thresholdValue );
					default:
						 throw new System.ArgumentException( "Invalid log pruning configuration value '" + originalConfigValue + "'. Invalid type '" + value.Type + "', valid are files, size, txs, entries, hours, days." );
			  }
		 }
	}

}