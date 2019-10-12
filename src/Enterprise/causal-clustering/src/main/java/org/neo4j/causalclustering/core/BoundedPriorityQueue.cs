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
namespace Neo4Net.causalclustering.core
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.BoundedPriorityQueue.Result.E_COUNT_EXCEEDED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.BoundedPriorityQueue.Result.E_SIZE_EXCEEDED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.BoundedPriorityQueue.Result.OK;

	/// <summary>
	/// A bounded queue which is bounded both by the count of elements and by the total
	/// size of all elements. The queue also has a minimum count which allows the queue
	/// to always allow a minimum number of items, regardless of total size.
	/// </summary>
	/// @param <E> element type </param>
	public class BoundedPriorityQueue<E>
	{
		 public class Config
		 {
			  internal readonly int MinCount;
			  internal readonly int MaxCount;
			  internal readonly long MaxBytes;

			  public Config( int maxCount, long maxBytes ) : this( 1, maxCount, maxBytes )
			  {
			  }

			  public Config( int minCount, int maxCount, long maxBytes )
			  {
					this.MinCount = minCount;
					this.MaxCount = maxCount;
					this.MaxBytes = maxBytes;
			  }
		 }

		 public interface Removable<E>
		 {
			  E Get();

			  bool Remove();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default <T> Removable<T> map(System.Func<E, T> fn)
	//		  {
	//				return new Removable<T>()
	//				{
	//					 @@Override public T get()
	//					 {
	//						  return fn.apply(Removable.this.get());
	//					 }
	//
	//					 @@Override public boolean remove()
	//					 {
	//						  return Removable.this.remove();
	//					 }
	//				};
	//		  }
		 }

		 public enum Result
		 {
			  Ok,
			  ECountExceeded,
			  ESizeExceeded
		 }

		 private readonly Config _config;
		 private readonly System.Func<E, long> _sizeOf;

		 private readonly BlockingQueue<StableElement> _queue;
		 private readonly AtomicLong _seqGen = new AtomicLong();
		 private readonly AtomicInteger _count = new AtomicInteger();
		 private readonly AtomicLong _bytes = new AtomicLong();

		 internal BoundedPriorityQueue( Config config, System.Func<E, long> sizeOf, IComparer<E> comparator )
		 {
			  this._config = config;
			  this._sizeOf = sizeOf;
			  this._queue = new PriorityBlockingQueue<StableElement>( config.MaxCount, new Comparator( this, comparator ) );
		 }

		 public virtual int Count()
		 {
			  return _count.get();
		 }

		 public virtual long Bytes()
		 {
			  return _bytes.get();
		 }

		 /// <summary>
		 /// Offers an element to the queue which gets accepted if neither the
		 /// element count nor the total byte limits are broken.
		 /// </summary>
		 /// <param name="element"> The element offered. </param>
		 /// <returns> OK if successful, and a specific error code otherwise. </returns>
		 public virtual Result Offer( E element )
		 {
			  int updatedCount = _count.incrementAndGet();
			  if ( updatedCount > _config.maxCount )
			  {
					_count.decrementAndGet();
					return E_COUNT_EXCEEDED;
			  }

			  long elementBytes = _sizeOf.apply( element );
			  long updatedBytes = _bytes.addAndGet( elementBytes );

			  if ( elementBytes != 0 && updatedCount > _config.minCount )
			  {
					if ( updatedBytes > _config.maxBytes )
					{
						 _bytes.addAndGet( -elementBytes );
						 _count.decrementAndGet();
						 return E_SIZE_EXCEEDED;
					}
			  }

			  if ( !_queue.offer( new StableElement( this, element ) ) )
			  {
					// this should not happen because we already have a reservation
					throw new System.InvalidOperationException();
			  }

			  return OK;
		 }

		 /// <summary>
		 /// Helper for deducting the element and byte counts for a removed element.
		 /// </summary>
		 private Optional<E> Deduct( StableElement element )
		 {
			  if ( element == null )
			  {
					return null;
			  }
			  _count.decrementAndGet();
			  _bytes.addAndGet( -_sizeOf.apply( element.Element ) );
			  return element.Element;
		 }

		 public virtual Optional<E> Poll()
		 {
			  return Deduct( _queue.poll() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Optional<E> poll(int timeout, java.util.concurrent.TimeUnit unit) throws InterruptedException
		 public virtual Optional<E> Poll( int timeout, TimeUnit unit )
		 {
			  return Deduct( _queue.poll( timeout, unit ) );
		 }

		 internal virtual Optional<Removable<E>> Peek()
		 {
			  return Optional.ofNullable( _queue.peek() );
		 }

		 internal class StableElement : Removable<E>
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal virtual void InitializeInstanceFields()
			 {
				 SeqNo = outerInstance.seqGen.AndIncrement;
			 }

			 private readonly BoundedPriorityQueue<E> _outerInstance;

			  internal long SeqNo;
			  internal readonly E Element;

			  internal StableElement( BoundedPriorityQueue<E> outerInstance, E element )
			  {
				  this._outerInstance = outerInstance;

				  if ( !InstanceFieldsInitialized )
				  {
					  InitializeInstanceFields();
					  InstanceFieldsInitialized = true;
				  }
					this.Element = element;
			  }

			  public override E Get()
			  {
					return Element;
			  }

			  public override bool Remove()
			  {
					bool removed = outerInstance.queue.remove( this );
					if ( removed )
					{
						 outerInstance.deduct( this );
					}
					return removed;
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					//noinspection unchecked
					StableElement that = ( StableElement ) o;
					return SeqNo == that.SeqNo;
			  }

			  public override int GetHashCode()
			  {
					return Objects.hash( SeqNo );
			  }
		 }

		 internal class Comparator : IComparer<BoundedPriorityQueue<E>.StableElement>
		 {
			 private readonly BoundedPriorityQueue<E> _outerInstance;

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal readonly IComparer<E> ComparatorConflict;

			  internal Comparator( BoundedPriorityQueue<E> outerInstance, IComparer<E> comparator )
			  {
				  this._outerInstance = outerInstance;
					this.ComparatorConflict = comparator;
			  }

			  public override int Compare( BoundedPriorityQueue<E>.StableElement o1, BoundedPriorityQueue<E>.StableElement o2 )
			  {
					int compare = ComparatorConflict.Compare( o1.element, o2.element );
					if ( compare != 0 )
					{
						 return compare;
					}
					return Long.compare( o1.seqNo, o2.seqNo );
			  }
		 }
	}

}