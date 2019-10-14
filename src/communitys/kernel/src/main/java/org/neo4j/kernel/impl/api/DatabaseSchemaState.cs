using System.Collections.Concurrent;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api
{

	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Used for the actual storage of "schema state".
	/// Schema state is transient state that should be invalidated when the schema changes.
	/// Examples of things stored in schema state is execution plans for cypher.
	/// </summary>
	public class DatabaseSchemaState : SchemaState
	{
		 private readonly IDictionary<object, object> _state;
		 private readonly Log _log;

		 public DatabaseSchemaState( LogProvider logProvider )
		 {
			  this._state = new ConcurrentDictionary<object, object>();
			  this._log = logProvider.getLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <K, V> V get(K key)
		 public override V Get<K, V>( K key )
		 {
			  return ( V ) _state[key];
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <K, V> V getOrCreate(K key, System.Func<K,V> creator)
		 public override V GetOrCreate<K, V>( K key, System.Func<K, V> creator )
		 {
			  return ( V ) _state.computeIfAbsent( key, ( System.Func<object, object> ) creator );
		 }

		 public override void Put<K, V>( K key, V value )
		 {
			  _state[key] = value;
		 }

		 public override void Clear()
		 {
			  _state.Clear();
			  _log.debug( "Schema state store has been cleared." );
		 }
	}

}