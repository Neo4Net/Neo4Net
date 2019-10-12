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
namespace Org.Neo4j.Consistency.repair
{
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;

	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;

	internal class RecordSet<R> : IEnumerable<R> where R : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
	{
		 private readonly MutableLongObjectMap<R> _map = new LongObjectHashMap<R>();

		 internal virtual void Add( R record )
		 {
			  _map.put( record.Id, record );
		 }

		 internal virtual RecordSet<R> Union( RecordSet<R> other )
		 {
			  RecordSet<R> set = new RecordSet<R>();
			  set.AddAll( this );
			  set.AddAll( other );
			  return set;
		 }

		 internal virtual int Size()
		 {
			  return _map.size();
		 }

		 public override IEnumerator<R> Iterator()
		 {
			  return _map.GetEnumerator();
		 }

		 public virtual void AddAll( RecordSet<R> other )
		 {
			  _map.putAll( other._map );
		 }

		 public virtual bool ContainsAll( RecordSet<R> other )
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			  return _map.Keys.containsAll( other._map.Keys );
		 }

		 public override string ToString()
		 {
			  StringBuilder builder = new StringBuilder( "[\n" );
			  foreach ( R r in _map.values() )
			  {
					builder.Append( r.ToString() ).Append(",\n");
			  }
			  return builder.Append( "]\n" ).ToString();
		 }
	}

}