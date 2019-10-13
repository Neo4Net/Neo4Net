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
namespace Neo4Net.causalclustering.core.replication.session
{

	/// <summary>
	/// Keeps a pool of local sub-sessions, to be used under a single global session. </summary>
	public class LocalSessionPool
	{
		 private readonly Deque<LocalSession> _sessionStack = new LinkedList<LocalSession>();

		 private readonly GlobalSession _globalSession;
		 private long _nextLocalSessionId;

		 public LocalSessionPool( GlobalSession globalSession )
		 {
			  this._globalSession = globalSession;
		 }

		 private LocalSession CreateSession()
		 {
			  return new LocalSession( _nextLocalSessionId++ );
		 }

		 public virtual GlobalSession GlobalSession
		 {
			 get
			 {
				  return _globalSession;
			 }
		 }

		 /// <summary>
		 /// Acquires a session and returns the next unique operation context
		 /// within that session. The session must be released when the operation
		 /// has been successfully finished. 
		 /// </summary>
		 public virtual OperationContext AcquireSession()
		 {
			 lock ( this )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LocalSession localSession = sessionStack.isEmpty() ? createSession() : sessionStack.pop();
				  LocalSession localSession = _sessionStack.Empty ? CreateSession() : _sessionStack.pop();
				  return new OperationContext( _globalSession, localSession.NextOperationId(), localSession );
			 }
		 }

		 /// <summary>
		 /// Releases a previously acquired session using the operation context
		 /// as a key. An unsuccessful operation should not be released, but it
		 /// will leak a local session.
		 /// 
		 /// The reason for not releasing an unsuccessful session is that operation
		 /// handlers might restrict sequence numbers to occur in strict order, and
		 /// thus an operation that it hasn't handled will block any future
		 /// operations under that session.
		 /// 
		 /// In general all operations should be retried until they do succeed, or
		 /// the entire session manager should eventually be restarted, thus
		 /// allocating a new global session to operate under.
		 /// </summary>
		 public virtual void ReleaseSession( OperationContext operationContext )
		 {
			 lock ( this )
			 {
				  _sessionStack.push( operationContext.LocalSession() );
			 }
		 }

		 public virtual long OpenSessionCount()
		 {
			 lock ( this )
			 {
				  return _nextLocalSessionId - _sessionStack.size();
			 }
		 }
	}

}