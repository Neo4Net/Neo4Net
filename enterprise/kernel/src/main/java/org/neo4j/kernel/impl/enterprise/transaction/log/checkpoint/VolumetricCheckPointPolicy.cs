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
namespace Org.Neo4j.Kernel.impl.enterprise.transaction.log.checkpoint
{

	using Service = Org.Neo4j.Helpers.Service;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using CheckPointThreshold = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointThreshold;
	using CheckPointThresholdPolicy = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointThresholdPolicy;
	using LogPruning = Org.Neo4j.Kernel.impl.transaction.log.pruning.LogPruning;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(CheckPointThresholdPolicy.class) public class VolumetricCheckPointPolicy extends org.neo4j.kernel.impl.transaction.log.checkpoint.CheckPointThresholdPolicy
	public class VolumetricCheckPointPolicy : CheckPointThresholdPolicy
	{
		 public VolumetricCheckPointPolicy() : base("volumetric")
		 {
		 }

		 public override CheckPointThreshold CreateThreshold( Config config, SystemNanoClock clock, LogPruning logPruning, LogProvider logProvider )
		 {
			  return new VolumetricCheckPointThreshold( logPruning );
		 }
	}

}