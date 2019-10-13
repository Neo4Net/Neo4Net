﻿using System.Collections.Generic;

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
namespace Neo4Net.cluster.com.message
{

	using Iterables = Neo4Net.Helpers.Collections.Iterables;

	public class TrackingMessageHolder : MessageHolder
	{
		 private readonly IList<Message> _messages = new List<Message>();

		 public override void Offer<T1>( Message<T1> message ) where T1 : MessageType
		 {
			  _messages.Add( message );
		 }

		 public virtual Message<T> Single<T>() where T : MessageType
		 {
			  return Iterables.single( _messages );
		 }

		 public virtual Message<T> First<T>() where T : MessageType
		 {
			  return Iterables.first( _messages );
		 }
	}

}