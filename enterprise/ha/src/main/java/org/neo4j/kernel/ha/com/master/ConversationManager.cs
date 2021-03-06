﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.ha.com.master
{

	using RequestContext = Org.Neo4j.com.RequestContext;
	using Org.Neo4j.Function;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConversationSPI = Org.Neo4j.Kernel.ha.cluster.ConversationSPI;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobHandle = Org.Neo4j.Scheduler.JobHandle;
	using ConcurrentAccessException = Org.Neo4j.Kernel.impl.util.collection.ConcurrentAccessException;
	using NoSuchEntryException = Org.Neo4j.Kernel.impl.util.collection.NoSuchEntryException;
	using Org.Neo4j.Kernel.impl.util.collection;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Clocks = Org.Neo4j.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.HaSettings.lock_read_timeout;

	/// <summary>
	/// Manages <seealso cref="Conversation"/> on master-side in HA.
	/// It's expected to have one instance of <seealso cref="ConversationManager"/> on master.
	/// 
	/// Used for keeping and monitoring clients <seealso cref="Conversation"/> on master side.
	/// </summary>
	public class ConversationManager : LifecycleAdapter
	{
		 private const int DEFAULT_TX_TIMEOUT_ADDITION = 5 * 1000;
		 private const int UNFINISHED_CONVERSATION_CLEANUP_DELAY = 1_000;

		 private readonly int _activityCheckIntervalMillis;
		 private readonly int _lockTimeoutAddition;
		 private readonly Config _config;
		 private readonly ConversationSPI _spi;
		 private readonly Factory<Conversation> conversationFactory = new FactoryAnonymousInnerClass();

		 private class FactoryAnonymousInnerClass : Factory<Conversation>
		 {
			 public Conversation newInstance()
			 {
				  return new Conversation( outerInstance.spi.acquireClient() );
			 }
		 }

		 internal TimedRepository<RequestContext, Conversation> Conversations;
		 private JobHandle _staleReaperJob;

		 /// <summary>
		 /// Build conversation manager with default values for activity check interval and timeout addition. </summary>
		 /// <param name="spi"> - conversation manager spi </param>
		 /// <param name="config"> - ha settings </param>
		 public ConversationManager( ConversationSPI spi, Config config ) : this( spi, config, UNFINISHED_CONVERSATION_CLEANUP_DELAY, DEFAULT_TX_TIMEOUT_ADDITION )
		 {
		 }

		 /// <summary>
		 /// Build conversation manager </summary>
		 /// <param name="spi"> - conversation manager spi </param>
		 /// <param name="config"> - ha settings </param>
		 /// <param name="activityCheckIntervalMillis"> - interval between conversations activity checking </param>
		 /// <param name="lockTimeoutAddition"> - addition to read timeout used to build conversation timeout </param>
		 public ConversationManager( ConversationSPI spi, Config config, int activityCheckIntervalMillis, int lockTimeoutAddition )
		 {
			  this._spi = spi;
			  this._config = config;
			  this._activityCheckIntervalMillis = activityCheckIntervalMillis;
			  this._lockTimeoutAddition = lockTimeoutAddition;
		 }

		 public override void Start()
		 {
			  Conversations = CreateConversationStore();
			  _staleReaperJob = _spi.scheduleRecurringJob( Group.SLAVE_LOCKS_TIMEOUT, _activityCheckIntervalMillis, Conversations );
		 }

		 public override void Stop()
		 {
			  _staleReaperJob.cancel( false );
			  Conversations = null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Conversation acquire(org.neo4j.com.RequestContext context) throws org.neo4j.kernel.impl.util.collection.NoSuchEntryException, org.neo4j.kernel.impl.util.collection.ConcurrentAccessException
		 public virtual Conversation Acquire( RequestContext context )
		 {
			  return Conversations.acquire( context );
		 }

		 public virtual void Release( RequestContext context )
		 {
			  Conversations.release( context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void begin(org.neo4j.com.RequestContext context) throws org.neo4j.kernel.impl.util.collection.ConcurrentAccessException
		 public virtual void Begin( RequestContext context )
		 {
			  Conversations.begin( context );
		 }

		 public virtual void End( RequestContext context )
		 {
			  Conversations.end( context );
		 }

		 public virtual ISet<RequestContext> ActiveContexts
		 {
			 get
			 {
				  return Conversations != null ? Conversations.keys() : Collections.emptySet();
			 }
		 }

		 /// <summary>
		 /// Stop conversation for specified context.
		 /// Conversation will still hold all already acquired locks, but will release all waiters and it will be
		 /// impossible to get new locks out of it. </summary>
		 /// <param name="context"> - context for which conversation should be stopped </param>
		 public virtual void Stop( RequestContext context )
		 {
			  Conversation conversation = Conversations.end( context );
			  if ( conversation != null && conversation.Active )
			  {
					conversation.Stop();
			  }
		 }

		 public virtual Conversation Acquire()
		 {
			  return ConversationFactory.newInstance();
		 }

		 protected internal virtual TimedRepository<RequestContext, Conversation> CreateConversationStore()
		 {
			  return new TimedRepository<RequestContext, Conversation>( ConversationFactory, ConversationReaper, _config.get( lock_read_timeout ).toMillis() + _lockTimeoutAddition, Clocks.systemClock() );
		 }

		 protected internal virtual System.Action<Conversation> ConversationReaper
		 {
			 get
			 {
				  return Conversation.close;
			 }
		 }

		 protected internal virtual Factory<Conversation> ConversationFactory
		 {
			 get
			 {
				  return conversationFactory;
			 }
		 }
	}

}