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
namespace Neo4Net.Kernel.impl.transaction.log.checkpoint
{
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Service = Neo4Net.Helpers.Service;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogPruning = Neo4Net.Kernel.impl.transaction.log.pruning.LogPruning;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.checkpoint.CheckPointThreshold.or;

	/// <summary>
	/// The {@code periodic} check point threshold policy uses the <seealso cref="GraphDatabaseSettings.check_point_interval_time"/>
	/// and <seealso cref="GraphDatabaseSettings.check_point_interval_tx"/> to decide when check points processes should be started.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(CheckPointThresholdPolicy.class) public class PeriodicThresholdPolicy extends CheckPointThresholdPolicy
	public class PeriodicThresholdPolicy : CheckPointThresholdPolicy
	{
		 public PeriodicThresholdPolicy() : base("periodic")
		 {
		 }

		 public override CheckPointThreshold CreateThreshold( Config config, SystemNanoClock clock, LogPruning logPruning, LogProvider logProvider )
		 {
			  int txThreshold = config.Get( GraphDatabaseSettings.check_point_interval_tx );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CountCommittedTransactionThreshold countCommittedTransactionThreshold = new CountCommittedTransactionThreshold(txThreshold);
			  CountCommittedTransactionThreshold countCommittedTransactionThreshold = new CountCommittedTransactionThreshold( txThreshold );

			  long timeMillisThreshold = config.Get( GraphDatabaseSettings.check_point_interval_time ).toMillis();
			  TimeCheckPointThreshold timeCheckPointThreshold = new TimeCheckPointThreshold( timeMillisThreshold, clock );

			  return or( countCommittedTransactionThreshold, timeCheckPointThreshold );
		 }
	}

}