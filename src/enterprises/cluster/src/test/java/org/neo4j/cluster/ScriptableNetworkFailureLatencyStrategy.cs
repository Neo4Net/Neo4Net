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
namespace Neo4Net.cluster
{

	using Neo4Net.cluster.com.message;
	using MessageType = Neo4Net.cluster.com.message.MessageType;

	/// <summary>
	/// Network failure strategy where you can declare, as the system runs,
	/// what failures exist in the system.
	/// </summary>
	public class ScriptableNetworkFailureLatencyStrategy : NetworkLatencyStrategy
	{
		 internal IList<string> NodesDown = new List<string>();
		 internal IList<string[]> LinksDown = new List<string[]>();

		 public virtual ScriptableNetworkFailureLatencyStrategy NodeIsDown( string id )
		 {
			  NodesDown.Add( id );
			  return this;
		 }

		 public virtual ScriptableNetworkFailureLatencyStrategy NodeIsUp( string id )
		 {
			  NodesDown.Remove( id );
			  return this;
		 }

		 public virtual ScriptableNetworkFailureLatencyStrategy LinkIsDown( string node1, string node2 )
		 {
			  LinksDown.Add( new string[]{ node1, node2 } );
			  LinksDown.Add( new string[]{ node2, node1 } );
			  return this;
		 }

		 public virtual ScriptableNetworkFailureLatencyStrategy LinkIsUp( string node1, string node2 )
		 {
			  LinksDown.Remove( new string[]{ node1, node2 } );
			  LinksDown.Remove( new string[]{ node2, node1 } );
			  return this;
		 }

		 public override long MessageDelay<T1>( Message<T1> message, string serverIdTo ) where T1 : Neo4Net.cluster.com.message.MessageType
		 {
			  if ( NodesDown.Contains( serverIdTo ) || NodesDown.Contains( message.GetHeader( Message.HEADER_FROM ) ) )
			  {
					return NetworkLatencyStrategy_Fields.LOST;
			  }

			  if ( LinksDown.Contains( new string[]{ message.GetHeader( Message.HEADER_FROM ), serverIdTo } ) )
			  {
					return NetworkLatencyStrategy_Fields.LOST;
			  }

			  return 0;
		 }
	}

}