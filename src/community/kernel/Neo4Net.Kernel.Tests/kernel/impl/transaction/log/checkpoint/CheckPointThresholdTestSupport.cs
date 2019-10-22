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
namespace Neo4Net.Kernel.impl.transaction.log.checkpoint
{
	using Before = org.junit.Before;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogPruning = Neo4Net.Kernel.impl.transaction.log.pruning.LogPruning;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;

	public class CheckPointThresholdTestSupport
	{
		 protected internal Config Config;
		 protected internal FakeClock Clock;
		 protected internal LogPruning LogPruning;
		 protected internal LogProvider LogProvider;
		 protected internal int? IntervalTx;
		 protected internal Duration IntervalTime;
		 protected internal System.Action<string> NotTriggered;
		 protected internal BlockingQueue<string> TriggerConsumer;
		 protected internal System.Action<string> Triggered;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  Config = Config.defaults();
			  Clock = Clocks.fakeClock();
			  LogPruning = LogPruning.NO_PRUNING;
			  LogProvider = NullLogProvider.Instance;
			  IntervalTx = Config.get( GraphDatabaseSettings.check_point_interval_tx );
			  IntervalTime = Config.get( GraphDatabaseSettings.check_point_interval_time );
			  TriggerConsumer = new LinkedBlockingQueue<string>();
			  Triggered = TriggerConsumer.offer;
			  NotTriggered = s => fail( "Should not have triggered: " + s );
		 }

		 protected internal virtual void WithPolicy( string policy )
		 {
			  Config.augment( stringMap( GraphDatabaseSettings.check_point_policy.name(), policy ) );
		 }

		 protected internal virtual void WithIntervalTime( string time )
		 {
			  Config.augment( stringMap( GraphDatabaseSettings.check_point_interval_time.name(), time ) );
		 }

		 protected internal virtual void WithIntervalTx( int count )
		 {
			  Config.augment( stringMap( GraphDatabaseSettings.check_point_interval_tx.name(), count.ToString() ) );
		 }

		 protected internal virtual CheckPointThreshold CreateThreshold()
		 {
			  return CheckPointThreshold.createThreshold( Config, Clock, LogPruning, LogProvider );
		 }

		 protected internal virtual void VerifyTriggered( string reason )
		 {
			  assertThat( TriggerConsumer.poll(), containsString(reason) );
		 }

		 protected internal virtual void VerifyNoMoreTriggers()
		 {
			  assertTrue( TriggerConsumer.Empty );
		 }
	}

}