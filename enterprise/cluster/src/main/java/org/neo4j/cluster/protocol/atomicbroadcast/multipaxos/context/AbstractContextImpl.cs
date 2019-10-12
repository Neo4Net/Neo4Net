using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.context
{

	using Org.Neo4j.cluster.com.message;
	using MessageType = Org.Neo4j.cluster.com.message.MessageType;
	using ClusterConfiguration = Org.Neo4j.cluster.protocol.cluster.ClusterConfiguration;
	using Timeouts = Org.Neo4j.cluster.timeout.Timeouts;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asList;

	/// <summary>
	/// This serves as a base class for contexts of distributed state machines, and holds
	/// various generally useful information, and provides access to logging.
	/// </summary>
	internal class AbstractContextImpl : TimeoutsContext, LoggingContext, ConfigurationContext
	{
		 protected internal readonly InstanceId Me;
		 protected internal readonly CommonContextState CommonState;
		 protected internal readonly LogProvider LogProvider;
		 protected internal readonly Timeouts Timeouts;

		 internal AbstractContextImpl( InstanceId me, CommonContextState commonState, LogProvider logProvider, Timeouts timeouts )
		 {
			  this.Me = me;
			  this.CommonState = commonState;
			  this.LogProvider = logProvider;
			  this.Timeouts = timeouts;
		 }

		 // LoggingContext
		 public override Log GetLog( Type loggingClass )
		 {
			  return LogProvider.getLog( loggingClass );
		 }

		 // TimeoutsContext
		 public override void SetTimeout<T1>( object key, Message<T1> timeoutMessage ) where T1 : Org.Neo4j.cluster.com.message.MessageType
		 {
			  Timeouts.setTimeout( key, timeoutMessage );
		 }

		 public override long GetTimeoutFor<T1>( Message<T1> timeoutMessage ) where T1 : Org.Neo4j.cluster.com.message.MessageType
		 {
			  return Timeouts.getTimeoutFor( timeoutMessage );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> cancelTimeout(Object key)
		 public override Message<MessageType> CancelTimeout( object key )
		 {
			  return Timeouts.cancelTimeout( key );
		 }

		 // ConfigurationContext
		 public virtual IList<URI> MemberURIs
		 {
			 get
			 {
				  return new IList<URI> { CommonState.configuration().MemberURIs };
			 }
		 }

		 public virtual InstanceId MyId
		 {
			 get
			 {
				  return Me;
			 }
		 }

		 public override URI BoundAt()
		 {
			  return CommonState.boundAt();
		 }

		 public virtual IDictionary<InstanceId, URI> Members
		 {
			 get
			 {
				  return CommonState.configuration().Members;
			 }
		 }

		 public virtual InstanceId Coordinator
		 {
			 get
			 {
				  return CommonState.configuration().getElected(ClusterConfiguration.COORDINATOR);
			 }
		 }

		 public override URI GetUriForId( InstanceId node )
		 {
			  return CommonState.configuration().getUriForId(node);
		 }

		 public override InstanceId GetIdForUri( URI uri )
		 {
			  return CommonState.configuration().getIdForUri(uri);
		 }

		 public override bool IsMe( InstanceId server )
		 {
			 lock ( this )
			 {
				  return Me.Equals( server );
			 }
		 }
	}

}