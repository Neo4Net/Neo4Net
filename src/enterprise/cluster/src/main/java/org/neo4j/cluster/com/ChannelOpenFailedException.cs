using System;

/*
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
namespace Neo4Net.cluster.com
{
	/// <summary>
	/// This is thrown when <seealso cref="NetworkSender"/> is unable to open a channel to another instance
	/// in the cluster.
	/// </summary>
	public class ChannelOpenFailedException : Exception
	{
		 public ChannelOpenFailedException() : base()
		 {
		 }

		 public ChannelOpenFailedException( string message ) : base( message )
		 {
		 }

		 public ChannelOpenFailedException( string message, Exception cause ) : base( message, cause )
		 {
		 }

		 public ChannelOpenFailedException( Exception cause ) : base( cause )
		 {
		 }
	}

}