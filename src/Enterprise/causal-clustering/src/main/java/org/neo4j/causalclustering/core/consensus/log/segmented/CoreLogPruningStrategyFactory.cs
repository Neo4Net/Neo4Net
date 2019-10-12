/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{
	using Neo4Net.Function;
	using ThresholdConfigParser = Neo4Net.Kernel.impl.transaction.log.pruning.ThresholdConfigParser;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.pruning.ThresholdConfigParser.parse;

	public class CoreLogPruningStrategyFactory : Factory<CoreLogPruningStrategy>
	{
		 private readonly string _pruningStrategyConfig;
		 private readonly LogProvider _logProvider;

		 public CoreLogPruningStrategyFactory( string pruningStrategyConfig, LogProvider logProvider )
		 {
			  this._pruningStrategyConfig = pruningStrategyConfig;
			  this._logProvider = logProvider;
		 }

		 public override CoreLogPruningStrategy NewInstance()
		 {
			  ThresholdConfigParser.ThresholdConfigValue thresholdConfigValue = parse( _pruningStrategyConfig );

			  string type = thresholdConfigValue.Type;
			  long value = thresholdConfigValue.Value;
			  switch ( type )
			  {
			  case "size":
					return new SizeBasedLogPruningStrategy( value );
			  case "txs":
			  case "entries": // txs and entries are synonyms
					return new EntryBasedLogPruningStrategy( value, _logProvider );
			  case "hours": // hours and days are currently not supported as such, default to no prune
			  case "days":
					throw new System.ArgumentException( "Time based pruning not supported yet for the segmented raft log, got '" + type + "'." );
			  case "false":
					return new NoPruningPruningStrategy();
			  default:
					throw new System.ArgumentException( "Invalid log pruning configuration value '" + value + "'. Invalid type '" + type + "', valid are files, size, txs, entries, hours, days." );
			  }
		 }
	}

}