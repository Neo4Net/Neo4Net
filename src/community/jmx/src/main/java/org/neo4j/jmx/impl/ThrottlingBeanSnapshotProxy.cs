using System;
using System.Collections.Generic;
using System.Reflection;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Jmx.impl
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.stream;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkArgument;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.requirePositive;

	/// <summary>
	/// The purpose of this proxy is to take a snapshot of all MBean attributes and return those cached values to prevent excessive resource consumption
	/// in case of frequent calls and expensive attribute calculations. Snapshot is updated no earlier than <seealso cref="updateInterval"/> ms after previous update.
	/// </summary>
	[Obsolete]
	internal class ThrottlingBeanSnapshotProxy : InvocationHandler
	{
		 private readonly ISet<System.Reflection.MethodInfo> _getters;
		 private readonly object _target;
		 private readonly Clock _clock;
		 private readonly object @lock = new object();
		 private readonly long _updateInterval;
		 private long _lastUpdateTime;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.Map<Method, ?> lastSnapshot;
		 private IDictionary<System.Reflection.MethodInfo, ?> _lastSnapshot;

		 private ThrottlingBeanSnapshotProxy<T>( Type iface, T target, long updateInterval, Clock clock )
		 {
				 iface = typeof( T );
			  this._getters = stream( iface.GetMethods( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance ) ).filter( m => m.ReturnType != Void.TYPE ).filter( m => m.ParameterCount == 0 ).collect( toSet() );
			  this._target = target;
			  this._updateInterval = updateInterval;
			  this._clock = clock;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object invoke(Object proxy, Method method, Object[] args) throws IllegalAccessException, InvocationTargetException
		 public override object Invoke( object proxy, System.Reflection.MethodInfo method, object[] args )
		 {
			  if ( !_getters.Contains( method ) )
			  {
					return method.invoke( _target, args );
			  }
			  lock ( @lock )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long now = clock.millis();
					long now = _clock.millis();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long age = now - lastUpdateTime;
					long age = now - _lastUpdateTime;
					if ( _lastSnapshot == null || age >= _updateInterval )
					{
						 _lastUpdateTime = now;
						 _lastSnapshot = TakeSnapshot();
					}
					return _lastSnapshot[method];
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.Map<Method, ?> takeSnapshot() throws InvocationTargetException, IllegalAccessException
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private IDictionary<System.Reflection.MethodInfo, ?> TakeSnapshot()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<Method, Object> snapshot = new java.util.HashMap<>();
			  IDictionary<System.Reflection.MethodInfo, object> snapshot = new Dictionary<System.Reflection.MethodInfo, object>();
			  foreach ( System.Reflection.MethodInfo getter in _getters )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Object value = getter.invoke(target);
					object value = getter.invoke( _target );
					snapshot[getter] = value;
			  }
			  return snapshot;
		 }

		 internal static I NewThrottlingBeanSnapshotProxy<I, T>( Type iface, T target, long updateInterval, Clock clock ) where T : I
		 {
				 iface = typeof( I );
			  if ( updateInterval == 0 )
			  {
					return target;
			  }
			  checkArgument( iface.IsInterface, "%s is not an interface", iface );
			  requirePositive( updateInterval );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ThrottlingBeanSnapshotProxy proxy = new ThrottlingBeanSnapshotProxy(iface, target, updateInterval, clock);
			  ThrottlingBeanSnapshotProxy proxy = new ThrottlingBeanSnapshotProxy( iface, target, updateInterval, clock );
			  return iface.cast( newProxyInstance( iface.ClassLoader, new Type[] { iface }, proxy ) );
		 }
	}

}