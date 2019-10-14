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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{

	using ValuesIterator = Neo4Net.Kernel.Api.Impl.Index.collector.ValuesIterator;

	/// <summary>
	/// Iterator over entity ids together with their respective score.
	/// </summary>
	public class ScoreEntityIterator : IEnumerator<ScoreEntityIterator.ScoreEntry>
	{
		 private readonly ValuesIterator _iterator;
		 private readonly System.Predicate<ScoreEntry> _predicate;
		 private ScoreEntry _next;

		 internal ScoreEntityIterator( ValuesIterator sortedValuesIterator )
		 {
			  this._iterator = sortedValuesIterator;
			  this._predicate = null;
		 }

		 private ScoreEntityIterator( ValuesIterator sortedValuesIterator, System.Predicate<ScoreEntry> predicate )
		 {
			  this._iterator = sortedValuesIterator;
			  this._predicate = predicate;
		 }

		 public virtual Stream<ScoreEntry> Stream()
		 {
			  return StreamSupport.stream( Spliterators.spliteratorUnknownSize( this, Spliterator.ORDERED ), false );
		 }

		 public override bool HasNext()
		 {
			  while ( _next == null && _iterator.hasNext() )
			  {
					long entityId = _iterator.next();
					float score = _iterator.currentScore();
					ScoreEntry tmp = new ScoreEntry( entityId, score );
					if ( _predicate == null || _predicate.test( tmp ) )
					{
						 _next = tmp;
					}
			  }
			  return _next != null;
		 }

		 public override ScoreEntry Next()
		 {
			  if ( HasNext() )
			  {
					ScoreEntry tmp = _next;
					_next = null;
					return tmp;
			  }
			  else
			  {
					throw new NoSuchElementException( "The iterator is exhausted" );
			  }
		 }

		 internal virtual ScoreEntityIterator Filter( System.Predicate<ScoreEntry> predicate )
		 {
			  if ( this._predicate != null )
			  {
					predicate = this._predicate.and( predicate );
			  }
			  return new ScoreEntityIterator( _iterator, predicate );
		 }

		 /// <summary>
		 /// Merges the given iterators into a single iterator, that maintains the aggregate descending score sort order.
		 /// </summary>
		 /// <param name="iterators"> to concatenate </param>
		 /// <returns> a <seealso cref="ScoreEntityIterator"/> that iterates over all of the elements in all of the given iterators </returns>
		 internal static ScoreEntityIterator MergeIterators( IList<ScoreEntityIterator> iterators )
		 {
			  return new ConcatenatingScoreEntityIterator( iterators );
		 }

		 private class ConcatenatingScoreEntityIterator : ScoreEntityIterator
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.List<? extends ScoreEntityIterator> iterators;
			  internal readonly IList<ScoreEntityIterator> Iterators;
			  internal readonly ScoreEntry[] Buffer;
			  internal bool Fetched;
			  internal ScoreEntry NextHead;

			  internal ConcatenatingScoreEntityIterator<T1>( IList<T1> iterators ) where T1 : ScoreEntityIterator : base( null )
			  {
					this.Iterators = iterators;
					this.Buffer = new ScoreEntry[iterators.Count];
			  }

			  public override bool HasNext()
			  {
					if ( !Fetched )
					{
						 Fetch();
					}
					return NextHead != null;
			  }

			  internal virtual void Fetch()
			  {
					int candidateHead = -1;
					for ( int i = 0; i < Iterators.Count; i++ )
					{
						 ScoreEntry entry = Buffer[i];
						 //Fill buffer if needed.
						 if ( entry == null && Iterators[i].hasNext() )
						 {
							  entry = Iterators[i].next();
							  Buffer[i] = entry;
						 }

						 //Check if entry might be candidate for next to return.
						 if ( entry != null && ( NextHead == null || entry.ScoreConflict > NextHead.score ) )
						 {
							  NextHead = entry;
							  candidateHead = i;
						 }
					}
					if ( candidateHead != -1 )
					{
						 Buffer[candidateHead] = null;
					}
					Fetched = true;
			  }

			  public override ScoreEntry Next()
			  {
					if ( HasNext() )
					{
						 Fetched = false;
						 ScoreEntry best = NextHead;
						 NextHead = null;
						 return best;
					}
					else
					{
						 throw new NoSuchElementException( "The iterator is exhausted" );
					}
			  }
		 }

		 /// <summary>
		 /// A ScoreEntry consists of an entity id together with its score.
		 /// </summary>
		 internal class ScoreEntry
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long EntityIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly float ScoreConflict;

			  internal virtual long EntityId()
			  {
					return EntityIdConflict;
			  }

			  internal virtual float Score()
			  {
					return ScoreConflict;
			  }

			  internal ScoreEntry( long entityId, float score )
			  {
					this.EntityIdConflict = entityId;
					this.ScoreConflict = score;
			  }

			  public override string ToString()
			  {
					return "ScoreEntry[entityId=" + EntityIdConflict + ", score=" + ScoreConflict + "]";
			  }
		 }
	}

}