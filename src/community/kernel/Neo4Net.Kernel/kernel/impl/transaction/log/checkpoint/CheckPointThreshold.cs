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

	using Config = Neo4Net.Kernel.configuration.Config;
	using LogPruning = Neo4Net.Kernel.impl.transaction.log.pruning.LogPruning;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.check_point_policy;


	/// <summary>
	/// A check point threshold provides information if a check point is required or not.
	/// </summary>
	public interface ICheckPointThreshold
	{

		 /// <summary>
		 /// This method initialize the threshold by providing the initial transaction id
		 /// </summary>
		 /// <param name="transactionId"> the latest transaction committed id </param>
		 void Initialize( long transactionId );

		 /// <summary>
		 /// This method can be used for querying the threshold about the necessity of a check point.
		 /// </summary>
		 /// <param name="lastCommittedTransactionId"> the latest transaction committed id </param>
		 /// <param name="consumer"> will be called with the description about this threshold only if the return value is true </param>
		 /// <returns> true is a check point is needed, false otherwise. </returns>
		 bool IsCheckPointingNeeded( long lastCommittedTransactionId, System.Action<string> consumer );

		 /// <summary>
		 /// This method notifies the threshold that a check point has happened. This must be called every time a check point
		 /// has been written in the transaction log in order to make sure that the threshold updates its condition.
		 /// <para>
		 /// This is important since we might have multiple thresholds or forced check points.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="transactionId"> the latest transaction committed id used by the check point </param>
		 void CheckPointHappened( long transactionId );

		 /// <summary>
		 /// Return a desired checking frequency, as a number of milliseconds between calls to
		 /// <seealso cref="isCheckPointingNeeded(long, Consumer)"/>.
		 /// </summary>
		 /// <returns> A desired scheduling frequency in milliseconds. </returns>
		 long CheckFrequencyMillis();

		 /// <summary>
		 /// Create and configure a <seealso cref="CheckPointThreshold"/> based on the given configurations.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static CheckPointThreshold createThreshold(org.Neo4Net.kernel.configuration.Config config, org.Neo4Net.time.SystemNanoClock clock, org.Neo4Net.kernel.impl.transaction.log.pruning.LogPruning logPruning, org.Neo4Net.logging.LogProvider logProvider)
	//	 {
	//		  String policyName = config.get(check_point_policy);
	//		  CheckPointThresholdPolicy policy;
	//		  try
	//		  {
	//				policy = CheckPointThresholdPolicy.loadPolicy(policyName);
	//		  }
	//		  catch (NoSuchElementException e)
	//		  {
	//				logProvider.getLog(CheckPointThreshold.class).warn("Could not load check point policy '" + check_point_policy.name() + "=" + policyName + "'. " + "Using default policy instead.", e);
	//				policy = new PeriodicThresholdPolicy();
	//		  }
	//		  return policy.createThreshold(config, clock, logPruning, logProvider);
	//	 }

		 /// <summary>
		 /// Create a new <seealso cref="CheckPointThreshold"/> which will trigger if any of the given thresholds triggers.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static CheckPointThreshold or(final CheckPointThreshold... thresholds)
	//	 {
	//		  return new CheckPointThreshold()
	//		  {
	//				@@Override public void initialize(long transactionId)
	//				{
	//					 for (CheckPointThreshold threshold : thresholds)
	//					 {
	//						  threshold.initialize(transactionId);
	//					 }
	//				}
	//
	//				@@Override public boolean isCheckPointingNeeded(long transactionId, Consumer<String> consumer)
	//				{
	//					 for (CheckPointThreshold threshold : thresholds)
	//					 {
	//						  if (threshold.isCheckPointingNeeded(transactionId, consumer))
	//						  {
	//								return true;
	//						  }
	//					 }
	//
	//					 return false;
	//				}
	//
	//				@@Override public void checkPointHappened(long transactionId)
	//				{
	//					 for (CheckPointThreshold threshold : thresholds)
	//					 {
	//						  threshold.checkPointHappened(transactionId);
	//					 }
	//				}
	//
	//				@@Override public long checkFrequencyMillis()
	//				{
	//					 return Stream.of(thresholds).mapToLong(CheckPointThreshold::checkFrequencyMillis).min().orElse(DEFAULT_CHECKING_FREQUENCY_MILLIS);
	//				}
	//		  };
	//	 }
	}

	public static class CheckPointThreshold_Fields
	{
		 public static readonly long DefaultCheckingFrequencyMillis = TimeUnit.SECONDS.toMillis( 10 );
	}

}