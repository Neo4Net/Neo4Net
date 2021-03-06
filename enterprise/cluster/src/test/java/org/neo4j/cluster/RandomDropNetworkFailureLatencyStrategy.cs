﻿using System;

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
namespace Org.Neo4j.cluster
{

	using Org.Neo4j.cluster.com.message;
	using MessageType = Org.Neo4j.cluster.com.message.MessageType;

	/// <summary>
	/// Randomly drops messages.
	/// </summary>
	public class RandomDropNetworkFailureLatencyStrategy : NetworkLatencyStrategy
	{
		 internal Random Random;
		 private double _rate;

		 /// 
		 /// <param name="seed"> Provide a seed for the Random, in order to produce repeatable tests. </param>
		 /// <param name="rate"> 1.0=no dropped messages, 0.0=all messages are lost </param>
		 public RandomDropNetworkFailureLatencyStrategy( long seed, double rate )
		 {
			  Rate = rate;
			  this.Random = new Random( seed );
		 }

		 public virtual double Rate
		 {
			 set
			 {
				  this._rate = value;
			 }
		 }

		 public override long MessageDelay<T1>( Message<T1> message, string serverIdTo ) where T1 : Org.Neo4j.cluster.com.message.MessageType
		 {
			  return Random.NextDouble() > _rate ? 0 : NetworkLatencyStrategy_Fields.LOST;
		 }
	}

}