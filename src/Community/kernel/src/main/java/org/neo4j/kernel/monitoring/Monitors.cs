using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.monitoring
{
	using ClassUtils = org.apache.commons.lang3.ClassUtils;
	using MutableBag = org.eclipse.collections.api.bag.MutableBag;
	using MultiReaderHashBag = org.eclipse.collections.impl.bag.mutable.MultiReaderHashBag;


	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.isEmpty;

	/// <summary>
	/// This can be used to create monitor instances using a Dynamic Proxy, which when invoked can delegate to any number of
	/// listeners. Listeners also implement the monitor interface.
	/// 
	/// The creation of monitors and registration of listeners may happen in any order. Listeners can be registered before
	/// creating the actual monitor, and vice versa.
	/// 
	/// Components that actually implement listening functionality must be registered using {<seealso cref="addMonitorListener(object, string...)"/>.
	/// 
	/// This class is thread-safe.
	/// </summary>
	public class Monitors
	{
		 /// <summary>
		 /// Monitor interface method -> Listeners </summary>
		 private readonly IDictionary<System.Reflection.MethodInfo, ISet<MonitorListenerInvocationHandler>> _methodMonitorListeners = new ConcurrentDictionary<System.Reflection.MethodInfo, ISet<MonitorListenerInvocationHandler>>();
		 private readonly MutableBag<Type> _monitoredInterfaces = MultiReaderHashBag.newBag();
		 private readonly Monitors _parent;

		 public Monitors() : this(null)
		 {
		 }

		 /// <summary>
		 /// Create a child monitor with a given {@code parent}. Propagation works as expected where you can subscribe to
		 /// global monitors through the child monitor, but not the other way around. E.g. you can not subscribe to monitors
		 /// that are registered on the child monitor through the parent monitor.
		 /// <para>
		 /// Events will bubble up from the children in a way that listeners on the child monitor will be invoked before the
		 /// parent ones.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="parent"> to propagate events to and from. </param>
		 public Monitors( Monitors parent )
		 {
			  this._parent = parent;
		 }

		 public virtual T NewMonitor<T>( Type monitorClass, params string[] tags )
		 {
				 monitorClass = typeof( T );
			  RequireInterface( monitorClass );
			  ClassLoader classLoader = monitorClass.ClassLoader;
			  MonitorInvocationHandler monitorInvocationHandler = new MonitorInvocationHandler( this, tags );
			  return monitorClass.cast( Proxy.newProxyInstance( classLoader, new Type[]{ monitorClass }, monitorInvocationHandler ) );
		 }

		 public virtual void AddMonitorListener( object monitorListener, params string[] tags )
		 {
			  MonitorListenerInvocationHandler monitorListenerInvocationHandler = CreateInvocationHandler( monitorListener, tags );

			  IList<Type> listenerInterfaces = GetAllInterfaces( monitorListener );
			  MethodsStream( listenerInterfaces ).forEach(method =>
			  {
				ISet<MonitorListenerInvocationHandler> methodHandlers = _methodMonitorListeners.computeIfAbsent( method, f => Collections.newSetFromMap( new ConcurrentDictionary<MonitorListenerInvocationHandler>() ) );
				methodHandlers.add( monitorListenerInvocationHandler );
			  });
			  _monitoredInterfaces.addAll( listenerInterfaces );
		 }

		 public virtual void RemoveMonitorListener( object monitorListener )
		 {
			  IList<Type> listenerInterfaces = GetAllInterfaces( monitorListener );
			  MethodsStream( listenerInterfaces ).forEach( method => cleanupMonitorListeners( monitorListener, method ) );
			  listenerInterfaces.ForEach( _monitoredInterfaces.remove );
		 }

		 public virtual bool HasListeners( Type monitorClass )
		 {
			  return _monitoredInterfaces.contains( monitorClass ) || ( ( _parent != null ) && _parent.hasListeners( monitorClass ) );
		 }

		 private void CleanupMonitorListeners( object monitorListener, System.Reflection.MethodInfo key )
		 {
			  _methodMonitorListeners.computeIfPresent(key, (method1, handlers) =>
			  {
				handlers.removeIf( handler => monitorListener.Equals( handler.MonitorListener ) );
				return handlers.Empty ? null : handlers;
			  });
		 }

		 private static IList<Type> GetAllInterfaces( object monitorListener )
		 {
			  return ClassUtils.getAllInterfaces( monitorListener.GetType() );
		 }

		 private static Stream<System.Reflection.MethodInfo> MethodsStream( IList<Type> interfaces )
		 {
			  return interfaces.Select( Type.getMethods ).flatMap( Arrays.stream );
		 }

		 private static MonitorListenerInvocationHandler CreateInvocationHandler( object monitorListener, string[] tags )
		 {
			  return isEmpty( tags ) ? new UntaggedMonitorListenerInvocationHandler( monitorListener ) : new TaggedMonitorListenerInvocationHandler( monitorListener, tags );
		 }

		 private static void RequireInterface( Type monitorClass )
		 {
			  if ( !monitorClass.IsInterface )
			  {
					throw new System.ArgumentException( "Interfaces should be provided." );
			  }
		 }
		 private interface MonitorListenerInvocationHandler
		 {
			  object MonitorListener { get; }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void invoke(Object proxy, Method method, Object[] args, String... tags) throws Throwable;
			  void Invoke( object proxy, System.Reflection.MethodInfo method, object[] args, params string[] tags );
		 }

		 private class UntaggedMonitorListenerInvocationHandler : MonitorListenerInvocationHandler
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly object MonitorListenerConflict;

			  internal UntaggedMonitorListenerInvocationHandler( object monitorListener )
			  {
					this.MonitorListenerConflict = monitorListener;
			  }

			  public virtual object MonitorListener
			  {
				  get
				  {
						return MonitorListenerConflict;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void invoke(Object proxy, Method method, Object[] args, String... tags) throws Throwable
			  public override void Invoke( object proxy, System.Reflection.MethodInfo method, object[] args, params string[] tags )
			  {
					method.invoke( MonitorListenerConflict, args );
			  }
		 }

		 private class TaggedMonitorListenerInvocationHandler : UntaggedMonitorListenerInvocationHandler
		 {
			  internal readonly string[] Tags;

			  internal TaggedMonitorListenerInvocationHandler( object monitorListener, params string[] tags ) : base( monitorListener )
			  {
					this.Tags = tags;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void invoke(Object proxy, Method method, Object[] args, String... tags) throws Throwable
			  public override void Invoke( object proxy, System.Reflection.MethodInfo method, object[] args, params string[] tags )
			  {
					if ( ArrayUtil.containsAll( this.Tags, tags ) )
					{
						 base.Invoke( proxy, method, args, tags );
					}
			  }
		 }

		 private class MonitorInvocationHandler : InvocationHandler
		 {
			  internal readonly Monitors Monitor;
			  internal readonly string[] Tags;

			  internal MonitorInvocationHandler( Monitors monitor, params string[] tags )
			  {
					this.Monitor = monitor;
					this.Tags = tags;
			  }

			  public override object Invoke( object proxy, System.Reflection.MethodInfo method, object[] args )
			  {
					InvokeMonitorListeners( Monitor, Tags, proxy, method, args );

					// Bubble up
					Monitors current = Monitor.parent;
					while ( current != null )
					{
						 InvokeMonitorListeners( current, Tags, proxy, method, args );
						 current = current._parent;
					}
					return null;
			  }

			  internal static void InvokeMonitorListeners( Monitors monitor, string[] tags, object proxy, System.Reflection.MethodInfo method, object[] args )
			  {
					ISet<MonitorListenerInvocationHandler> handlers = monitor._methodMonitorListeners[method];
					if ( handlers == null || handlers.Count == 0 )
					{
						 return;
					}
					foreach ( MonitorListenerInvocationHandler monitorListenerInvocationHandler in handlers )
					{
						 try
						 {
							  monitorListenerInvocationHandler.Invoke( proxy, method, args, tags );
						 }
						 catch ( Exception )
						 {
						 }
					}
			  }
		 }
	}

}