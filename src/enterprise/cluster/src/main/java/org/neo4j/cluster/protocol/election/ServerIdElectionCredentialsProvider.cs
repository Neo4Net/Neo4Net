﻿/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.cluster.protocol.election
{

	/// <summary>
	/// ElectionCredentialsProvider that provides the server URI as credentials
	/// for elections. Natural comparison of the URI string is used.
	/// </summary>
	public class ServerIdElectionCredentialsProvider : ElectionCredentialsProvider, BindingListener
	{
		 private volatile URI _me;

		 public override void ListeningAt( URI me )
		 {
			  this._me = me;
		 }

		 public override ElectionCredentials GetCredentials( string role )
		 {
			  return new ServerIdElectionCredentials( _me );
		 }
	}

}