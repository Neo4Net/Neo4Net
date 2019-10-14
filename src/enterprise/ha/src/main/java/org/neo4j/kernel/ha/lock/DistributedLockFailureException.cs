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
namespace Neo4Net.Kernel.ha.@lock
{
	using ComException = Neo4Net.com.ComException;
	using TransientTransactionFailureException = Neo4Net.Graphdb.TransientTransactionFailureException;
	using Master = Neo4Net.Kernel.ha.com.master.Master;

	/// <summary>
	/// Thrown upon network communication failures, when taking or releasing distributed locks in HA.
	/// </summary>
	public class DistributedLockFailureException : TransientTransactionFailureException
	{
		 public DistributedLockFailureException( string message, Master master, ComException cause ) : base( message + " uniquetempvar. The most common causes of this exception are " + "network failures, or master-switches where the failing transaction was started before the last " + "master election.", cause )
		 {
		 }
	}

}