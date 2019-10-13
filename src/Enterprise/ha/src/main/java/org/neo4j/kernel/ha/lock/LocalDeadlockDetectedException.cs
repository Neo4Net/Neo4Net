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

	using DumpLocksVisitor = Neo4Net.Kernel.impl.locking.DumpLocksVisitor;
	using LockType = Neo4Net.Kernel.impl.locking.LockType;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using FormattedLog = Neo4Net.Logging.FormattedLog;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;

	/// <summary>
	/// Temporary exception to aid in driving out a nasty "lock get stuck" issue in HA. Since it's subclasses
	/// <seealso cref="DeadlockDetectedException"/> it will be invisible to users and code that already handle such
	/// deadlock exceptions and retry. This exception is thrown instead of awaiting a lock locally on a slave
	/// after it was acquired on the master, since applying a lock locally after master granted it should succeed,
	/// or fail; it cannot wait for another condition.
	/// 
	/// While this work-around is in place there is more breathing room to figure out the real problem preventing
	/// some local locks to be grabbed.
	/// 
	/// @author Mattias Persson
	/// </summary>
	public class LocalDeadlockDetectedException : DeadlockDetectedException
	{
		 public LocalDeadlockDetectedException( Neo4Net.Kernel.impl.locking.Locks_Client lockClient, Locks lockManager, ResourceType resourceType, long resourceId, LockType type ) : base( ConstructHelpfulDiagnosticsMessage( lockClient, lockManager, resourceType, resourceId, type ) )
		 {
		 }

		 private static string ConstructHelpfulDiagnosticsMessage( Neo4Net.Kernel.impl.locking.Locks_Client client, Locks lockManager, ResourceType resourceType, long resourceId, LockType type )
		 {
			  StringWriter stringWriter = new StringWriter();
			  stringWriter.append( format( "%s tried to apply local %s lock on %s(%s) after acquired on master. Currently these locks exist:%n", client, type, resourceType, resourceId ) );

			  lockManager.Accept( new DumpLocksVisitor( FormattedLog.withUTCTimeZone().toWriter(stringWriter) ) );
			  return stringWriter.ToString();
		 }
	}

}