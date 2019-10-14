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
namespace Neo4Net.Collections
{

	using Resource = Neo4Net.Graphdb.Resource;
	using ResourceUtils = Neo4Net.Graphdb.ResourceUtils;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.resourceIterator;

	public class PrimitiveLongResourceCollections
	{
		 private static readonly PrimitiveLongResourceIterator EMPTY = new PrimitiveLongBaseResourceIteratorAnonymousInnerClass();

		 private class PrimitiveLongBaseResourceIteratorAnonymousInnerClass : PrimitiveLongBaseResourceIterator
		 {
			 public PrimitiveLongBaseResourceIteratorAnonymousInnerClass() : base(null)
			 {
			 }

			 protected internal override bool fetchNext()
			 {
				  return false;
			 }
		 }

		 public static PrimitiveLongResourceIterator EmptyIterator()
		 {
			  return EMPTY;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static PrimitiveLongResourceIterator iterator(org.neo4j.graphdb.Resource resource, final long... items)
		 public static PrimitiveLongResourceIterator Iterator( Resource resource, params long[] items )
		 {
			  return resourceIterator( PrimitiveLongCollections.Iterator( items ), resource );
		 }

		 public static PrimitiveLongResourceIterator Concat( params PrimitiveLongResourceIterator[] primitiveLongResourceIterators )
		 {
			  return Concat( Arrays.asList( primitiveLongResourceIterators ) );
		 }

		 public static PrimitiveLongResourceIterator Concat( IEnumerable<PrimitiveLongResourceIterator> primitiveLongResourceIterators )
		 {
			  return new PrimitiveLongConcatingResourceIterator( primitiveLongResourceIterators );
		 }

		 public abstract class PrimitiveLongBaseResourceIterator : PrimitiveLongCollections.PrimitiveLongBaseIterator, PrimitiveLongResourceIterator
		 {
			  internal Resource Resource;

			  public PrimitiveLongBaseResourceIterator( Resource resource )
			  {
					this.Resource = resource;
			  }

			  public override void Close()
			  {
					if ( Resource != null )
					{
						 Resource.close();
						 Resource = null;
					}
			  }
		 }

		 private class PrimitiveLongConcatingResourceIterator : PrimitiveLongCollections.PrimitiveLongConcatingIterator, PrimitiveLongResourceIterator
		 {
			  internal readonly IEnumerable<PrimitiveLongResourceIterator> Iterators;
			  internal volatile bool Closed;

			  internal PrimitiveLongConcatingResourceIterator( IEnumerable<PrimitiveLongResourceIterator> iterators ) : base( iterators.GetEnumerator() )
			  {
					this.Iterators = iterators;
			  }

			  protected internal override bool FetchNext()
			  {
					return !Closed && base.FetchNext();
			  }

			  public override void Close()
			  {
					if ( !Closed )
					{
						 Closed = true;
						 ResourceUtils.closeAll( Iterators );
					}
			  }
		 }
	}

}