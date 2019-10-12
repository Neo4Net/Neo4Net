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

	using Service = Neo4Net.Helpers.Service;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogPruning = Neo4Net.Kernel.impl.transaction.log.pruning.LogPruning;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

	/// <summary>
	/// The <seealso cref="CheckPointThresholdPolicy"/> specifies the overall <em>type</em> of threshold that should be used for
	/// deciding when to check point.
	/// 
	/// The is determined by the <seealso cref="org.neo4j.graphdb.factory.GraphDatabaseSettings.check_point_policy"/> setting, and
	/// based on this, the concrete policies are loaded and used to
	/// <seealso cref="CheckPointThreshold.createThreshold(Config, Clock, LogPruning, LogProvider) create"/> the final and fully
	/// configured check point thresholds.
	/// </summary>
	public abstract class CheckPointThresholdPolicy : Service
	{
		 /// <summary>
		 /// Create a new instance of a service implementation identified with the
		 /// specified key(s).
		 /// </summary>
		 /// <param name="key"> the main key for identifying this service implementation </param>
		 /// <param name="altKeys"> alternative spellings of the identifier of this service </param>
		 protected internal CheckPointThresholdPolicy( string key, params string[] altKeys ) : base( key, altKeys )
		 {
		 }

		 /// <summary>
		 /// Load the <seealso cref="CheckPointThresholdPolicy"/> by the given name.
		 /// </summary>
		 /// <exception cref="NoSuchElementException"> if the policy was not found. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static CheckPointThresholdPolicy loadPolicy(String policyName) throws java.util.NoSuchElementException
		 public static CheckPointThresholdPolicy LoadPolicy( string policyName )
		 {
			  return Service.load( typeof( CheckPointThresholdPolicy ), policyName );
		 }

		 /// <summary>
		 /// Create a <seealso cref="CheckPointThreshold"/> instance based on this policy and the given configurations.
		 /// </summary>
		 public abstract CheckPointThreshold CreateThreshold( Config config, SystemNanoClock clock, LogPruning logPruning, LogProvider logProvider );
	}

}