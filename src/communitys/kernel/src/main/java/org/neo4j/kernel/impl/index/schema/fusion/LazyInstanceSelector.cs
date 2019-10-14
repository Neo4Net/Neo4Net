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
namespace Neo4Net.Kernel.Impl.Index.Schema.fusion
{

	/// <summary>
	/// Selects an instance given a certain slot and instantiate them lazy. </summary>
	/// @param <T> type of instance </param>
	internal class LazyInstanceSelector<T> : InstanceSelector<T>
	{
		 private readonly System.Func<IndexSlot, T> _factory;

		 /// <summary>
		 /// Empty lazy selector
		 /// 
		 /// See <seealso cref="this.LazyInstanceSelector(System.Collections.Hashtable, Function)"/>
		 /// </summary>
		 internal LazyInstanceSelector( System.Func<IndexSlot, T> factory ) : this( new Dictionary<>( typeof( IndexSlot ) ), factory )
		 {
		 }

		 /// <summary>
		 /// Lazy selector with initial mapping
		 /// </summary>
		 /// <param name="map"> with initial mapping </param>
		 /// <param name="factory"> <seealso cref="Function"/> for instantiating instances for specific slots. </param>
		 internal LazyInstanceSelector( Dictionary<IndexSlot, T> map, System.Func<IndexSlot, T> factory ) : base( map )
		 {
			  this._factory = factory;
		 }

		 /// <summary>
		 /// Instantiating an instance if it hasn't already been instantiated.
		 /// 
		 /// See <seealso cref="InstanceSelector.select(IndexSlot)"/>
		 /// </summary>
		 internal override T Select( IndexSlot slot )
		 {
			  return Instances.computeIfAbsent(slot, s =>
			  {
				AssertOpen();
				return _factory.apply( s );
			  });
		 }

		 /// <summary>
		 /// Returns the instance at the given slot. If the instance at the given {@code slot} hasn't been instantiated yet, {@code null} is returned.
		 /// </summary>
		 /// <param name="slot"> slot to return instance for. </param>
		 /// <returns> the instance at the given {@code slot}, or {@code null}. </returns>
		 internal virtual T GetIfInstantiated( IndexSlot slot )
		 {
			  return Instances[slot];
		 }

		 private void AssertOpen()
		 {
			  if ( Closed )
			  {
					throw new System.InvalidOperationException( "This selector has been closed" );
			  }
		 }
	}

}