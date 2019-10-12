using System.Collections.Generic;
using System.Text;

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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.stats
{

	using Org.Neo4j.@unsafe.Impl.Batchimport.staging;

	/// <summary>
	/// Provides stats about a <seealso cref="Step"/>.
	/// </summary>
	public class StepStats : StatsProvider
	{
		 private readonly string _name;
		 private readonly bool _stillWorking;
		 private readonly ICollection<StatsProvider> _providers;

		 public StepStats( string name, bool stillWorking, ICollection<StatsProvider> providers )
		 {
			  this._name = name;
			  this._stillWorking = stillWorking;
			  this._providers = new List<StatsProvider>( providers );
		 }

		 public override Key[] Keys()
		 {
			  Key[] keys = null;
			  foreach ( StatsProvider provider in _providers )
			  {
					Key[] providerKeys = provider.Keys();
					if ( keys == null )
					{
						 keys = providerKeys;
					}
					else
					{
						 foreach ( Key providerKey in providerKeys )
						 {
							  if ( !ArrayContains( keys, providerKey ) )
							  {
									keys = Arrays.copyOf( keys, keys.Length + 1 );
									keys[keys.Length - 1] = providerKey;
							  }
						 }
					}
			  }
			  return keys;
		 }

		 private bool ArrayContains<T>( T[] array, T item )
		 {
			  foreach ( T arrayItem in array )
			  {
					if ( arrayItem.Equals( item ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override Stat Stat( Key key )
		 {
			  foreach ( StatsProvider provider in _providers )
			  {
					Stat stat = provider.Stat( key );
					if ( stat != null )
					{
						 return stat;
					}
			  }
			  return null;
		 }

		 public override string ToString()
		 {
			  return ToString( DetailLevel.Important );
		 }

		 public virtual string ToString( DetailLevel detailLevel )
		 {
			  StringBuilder builder = new StringBuilder();
			  if ( !_stillWorking && detailLevel == DetailLevel.Basic )
			  {
					builder.Append( " DONE" );
			  }

			  int i = 0;
			  foreach ( Key key in Keys() )
			  {
					Stat stat = stat( key );
					if ( ( int )detailLevel >= stat.DetailLevel().ordinal() )
					{
						 builder.Append( i++ > 0 ? " " : "" ).Append( !string.ReferenceEquals( key.ShortName(), null ) ? key.ShortName() + ":" : "" ).Append(stat);
					}
			  }
			  return _name + ( builder.Length > 0 ? ":" + builder : "" );
		 }
	}

}