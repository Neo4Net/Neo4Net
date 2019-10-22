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
namespace Neo4Net.cluster.protocol
{
	using Neo4Net.cluster.com.message;
	using MessageType = Neo4Net.cluster.com.message.MessageType;

	public interface TimeoutsContext
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void setTimeout(Object key, org.Neo4Net.cluster.com.message.Message<? extends org.Neo4Net.cluster.com.message.MessageType> timeoutMessage);
		 void setTimeout<T1>( object key, Message<T1> timeoutMessage );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.cluster.com.message.Message<? extends org.Neo4Net.cluster.com.message.MessageType> cancelTimeout(Object key);
		 Message<MessageType> CancelTimeout( object key );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: long getTimeoutFor(org.Neo4Net.cluster.com.message.Message<? extends org.Neo4Net.cluster.com.message.MessageType> timeoutMessage);
		 long getTimeoutFor<T1>( Message<T1> timeoutMessage );
	}

}