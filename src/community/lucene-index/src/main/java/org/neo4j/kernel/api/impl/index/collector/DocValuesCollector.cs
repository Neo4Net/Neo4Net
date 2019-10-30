using System;
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
namespace Neo4Net.Kernel.Api.Impl.Index.collector
{
	using Document = org.apache.lucene.document.Document;
	using DocValuesType = Org.Apache.Lucene.Index.DocValuesType;
	using FieldInfo = Org.Apache.Lucene.Index.FieldInfo;
	using LeafReader = Org.Apache.Lucene.Index.LeafReader;
	using LeafReaderContext = Org.Apache.Lucene.Index.LeafReaderContext;
	using NumericDocValues = Org.Apache.Lucene.Index.NumericDocValues;
	using ReaderUtil = Org.Apache.Lucene.Index.ReaderUtil;
	using Collector = org.apache.lucene.search.Collector;
	using ConstantScoreScorer = org.apache.lucene.search.ConstantScoreScorer;
	using DocIdSet = org.apache.lucene.search.DocIdSet;
	using DocIdSetIterator = org.apache.lucene.search.DocIdSetIterator;
	using LeafCollector = org.apache.lucene.search.LeafCollector;
	using ScoreDoc = org.apache.lucene.search.ScoreDoc;
	using Scorer = org.apache.lucene.search.Scorer;
	using SimpleCollector = org.apache.lucene.search.SimpleCollector;
	using Sort = org.apache.lucene.search.Sort;
	using TopDocs = org.apache.lucene.search.TopDocs;
	using TopFieldCollector = org.apache.lucene.search.TopFieldCollector;
	using TopScoreDocCollector = org.apache.lucene.search.TopScoreDocCollector;
	using ArrayUtil = org.apache.lucene.util.ArrayUtil;
	using DocIdSetBuilder = org.apache.lucene.util.DocIdSetBuilder;


	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using Neo4Net.GraphDb.Index;
	using Neo4Net.Collections.Helpers;
	using Neo4Net.Collections.Helpers;
	using Neo4Net.Index.impl.lucene.@explicit;
	using Neo4Net.Kernel.Impl.Api.explicitindex;
	using IndexProgressor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// Collector to record per-segment {@code DocIdSet}s and {@code LeafReaderContext}s for every
	/// segment that contains a hit. Those items can be later used to read {@code DocValues} fields
	/// and iterate over the matched {@code DocIdSet}s. This collector is different from
	/// {@code org.apache.lucene.search.CachingCollector} in that the later focuses on predictable RAM usage
	/// and feeding other collectors while this collector focuses on exposing the required per-segment data structures
	/// to the user.
	/// </summary>
	public class DocValuesCollector : SimpleCollector
	{
		 private static readonly EmptyIndexHits<Document> _emptyIndexHits = new EmptyIndexHits<Document>();

		 private LeafReaderContext _context;
		 private int _segmentHits;
		 private int _totalHits;
		 private Scorer _scorer;
		 private float[] _scores;
		 private readonly bool _keepScores;
		 private readonly IList<MatchingDocs> _matchingDocs = new List<MatchingDocs>();
		 private Docs _docs;

		 /// <summary>
		 /// Default Constructor, does not keep scores.
		 /// </summary>
		 public DocValuesCollector() : this(false)
		 {
		 }

		 /// <param name="keepScores"> true if you want to trade correctness for speed </param>
		 public DocValuesCollector( bool keepScores )
		 {
			  this._keepScores = keepScores;
		 }

		 /// <param name="field"> the field that contains the values </param>
		 /// <returns> an iterator over all NumericDocValues from the given field </returns>
		 public virtual LongValuesIterator GetValuesIterator( string field )
		 {
			  return new LongValuesIterator( GetMatchingDocs(), TotalHits, field );
		 }

		 public virtual IndexProgressor GetIndexProgressor( string field, Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor_NodeValueClient client )
		 {
			  return new LongValuesIndexProgressor( GetMatchingDocs(), TotalHits, field, client );
		 }

		 /// <param name="field"> the field that contains the values </param>
		 /// <param name="sort"> how the results should be sorted </param>
		 /// <returns> an iterator over all NumericDocValues from the given field with respect to the given sort </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ValuesIterator getSortedValuesIterator(String field, org.apache.lucene.search.Sort sort) throws java.io.IOException
		 public virtual ValuesIterator GetSortedValuesIterator( string field, Sort sort )
		 {
			  if ( sort == null || sort == Sort.INDEXORDER )
			  {
					return GetValuesIterator( field );
			  }
			  int size = TotalHits;
			  if ( size == 0 )
			  {
					return ValuesIterator.EMPTY;
			  }
			  TopDocs topDocs = GetTopDocs( sort, size );
			  LeafReaderContext[] contexts = GetLeafReaderContexts( GetMatchingDocs() );
			  return new TopDocsValuesIterator( topDocs, contexts, field );
		 }

		 /// <summary>
		 /// Replay the search and collect every hit into TopDocs. One {@code ScoreDoc} is allocated
		 /// for every hit and the {@code Document} instance is loaded lazily with on every iteration step.
		 /// </summary>
		 /// <param name="sort"> how to sort the iterator. If this is null, results will be in index-order. </param>
		 /// <returns> an indexhits iterator over all matches </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.GraphDb.Index.IndexHits<org.apache.lucene.document.Document> getIndexHits(org.apache.lucene.search.Sort sort) throws java.io.IOException
		 public virtual IndexHits<Document> GetIndexHits( Sort sort )
		 {
			  IList<MatchingDocs> matchingDocs = GetMatchingDocs();
			  int size = TotalHits;
			  if ( size == 0 )
			  {
					return _emptyIndexHits;
			  }

			  if ( sort == null || sort == Sort.INDEXORDER )
			  {
					return new DocsInIndexOrderIterator( matchingDocs, size, KeepScores );
			  }

			  TopDocs topDocs = GetTopDocs( sort, size );
			  LeafReaderContext[] contexts = GetLeafReaderContexts( matchingDocs );
			  return new TopDocsIterator( topDocs, contexts );
		 }

		 /// <returns> the total number of hits across all segments. </returns>
		 public virtual int TotalHits
		 {
			 get
			 {
				  return _totalHits;
			 }
		 }

		 /// <returns> true if scores were saved. </returns>
		 public virtual bool KeepScores
		 {
			 get
			 {
				  return _keepScores;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void collect(int doc) throws java.io.IOException
		 public override void Collect( int doc )
		 {
			  _docs.addDoc( doc );
			  if ( _keepScores )
			  {
					if ( _segmentHits >= _scores.Length )
					{
						 float[] newScores = new float[ArrayUtil.oversize( _segmentHits + 1, 4 )];
						 Array.Copy( _scores, 0, newScores, 0, _segmentHits );
						 _scores = newScores;
					}
					_scores[_segmentHits] = _scorer.score();
			  }
			  _segmentHits++;
			  _totalHits++;
		 }

		 public override bool NeedsScores()
		 {
			  return _keepScores;
		 }

		 public override Scorer Scorer
		 {
			 set
			 {
				  this._scorer = value;
			 }
		 }

		 public override void DoSetNextReader( LeafReaderContext context )
		 {
			  if ( _docs != null && _segmentHits > 0 )
			  {
					CreateMatchingDocs();
			  }
			  int maxDoc = context.reader().maxDoc();
			  _docs = CreateDocs( maxDoc );
			  if ( _keepScores )
			  {
					int initialSize = Math.Min( 32, maxDoc );
					_scores = new float[initialSize];
			  }
			  _segmentHits = 0;
			  this._context = context;
		 }

		 /// <returns> the documents matched by the query, one <seealso cref="MatchingDocs"/> per visited segment that contains a hit. </returns>
		 public virtual IList<MatchingDocs> GetMatchingDocs()
		 {
			  if ( _docs != null && _segmentHits > 0 )
			  {
					CreateMatchingDocs();
					_docs = null;
					_scores = null;
					_context = null;
			  }

			  return Collections.unmodifiableList( _matchingDocs );
		 }

		 /// <returns> a new <seealso cref="Docs"/> to record hits. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Docs createDocs(final int maxDoc)
		 private Docs CreateDocs( int maxDoc )
		 {
			  return new Docs( maxDoc );
		 }

		 private void CreateMatchingDocs()
		 {
			  if ( _scores == null || _scores.Length == _segmentHits )
			  {
					_matchingDocs.Add( new MatchingDocs( this._context, _docs.DocIdSet, _segmentHits, _scores ) );
			  }
			  else
			  {
					// NOTE: we could skip the copy step here since the MatchingDocs are supposed to be
					// consumed through any of the provided Iterators (actually, the replay method),
					// which all don't care if scores has null values at the end.
					// This is for just sanity's sake, we could also make MatchingDocs private
					// and treat this as implementation detail.
					float[] finalScores = new float[_segmentHits];
					Array.Copy( _scores, 0, finalScores, 0, _segmentHits );
					_matchingDocs.Add( new MatchingDocs( this._context, _docs.DocIdSet, _segmentHits, finalScores ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.lucene.search.TopDocs getTopDocs(org.apache.lucene.search.Sort sort, int size) throws java.io.IOException
		 private TopDocs GetTopDocs( Sort sort, int size )
		 {
			  TopDocs topDocs;
			  if ( sort == Sort.RELEVANCE )
			  {
					TopScoreDocCollector collector = TopScoreDocCollector.create( size );
					ReplayTo( collector );
					topDocs = collector.topDocs();
			  }
			  else
			  {
					TopFieldCollector collector = TopFieldCollector.create( sort, size, false, true, false );
					ReplayTo( collector );
					topDocs = collector.topDocs();
			  }
			  return topDocs;
		 }

		 private static LeafReaderContext[] GetLeafReaderContexts( IList<MatchingDocs> matchingDocs )
		 {
			  int segments = matchingDocs.Count;
			  LeafReaderContext[] contexts = new LeafReaderContext[segments];
			  for ( int i = 0; i < segments; i++ )
			  {
					MatchingDocs matchingDoc = matchingDocs[i];
					contexts[i] = matchingDoc.Context;
			  }
			  return contexts;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void replayTo(org.apache.lucene.search.Collector collector) throws java.io.IOException
		 private void ReplayTo( Collector collector )
		 {
			  foreach ( MatchingDocs docs in GetMatchingDocs() )
			  {
					LeafCollector leafCollector = collector.getLeafCollector( docs.Context );
					Scorer scorer;
					DocIdSetIterator idIterator = docs.DocIdSet.GetEnumerator();
					if ( KeepScores )
					{
						 scorer = new ReplayingScorer( docs.Scores );
					}
					else
					{
						 scorer = new ConstantScoreScorer( null, Float.NaN, idIterator );
					}
					leafCollector.Scorer = scorer;
					int doc;
					while ( ( doc = idIterator.nextDoc() ) != DocIdSetIterator.NO_MORE_DOCS )
					{
						 leafCollector.collect( doc );
					}
			  }
		 }

		 /// <summary>
		 /// Iterates over all per-segment <seealso cref="DocValuesCollector.MatchingDocs"/>.
		 /// Provides base functionality for extracting IEntity ids and other values from documents.
		 /// </summary>
		 private abstract class LongValuesSource
		 {
			  internal readonly IEnumerator<DocValuesCollector.MatchingDocs> MatchingDocs;
			  internal readonly string Field;
			  internal readonly int TotalHits;
			  internal readonly IDictionary<string, NumericDocValues> DocValuesCache;

			  internal DocIdSetIterator CurrentIdIterator;
			  internal NumericDocValues CurrentDocValues;
			  internal DocValuesCollector.MatchingDocs CurrentDocs;
			  internal int Index;
			  internal long Next;

			  internal LongValuesSource( IEnumerable<DocValuesCollector.MatchingDocs> allMatchingDocs, int totalHits, string field )
			  {
					this.TotalHits = totalHits;
					this.Field = field;
					MatchingDocs = allMatchingDocs.GetEnumerator();
					DocValuesCache = new Dictionary<string, NumericDocValues>();
			  }

			  /// <returns> true if it was able to make sure, that currentDisi is valid </returns>
			  internal virtual bool EnsureValidDisi()
			  {
					try
					{
						 while ( CurrentIdIterator == null )
						 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  if ( MatchingDocs.hasNext() )
							  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
									CurrentDocs = MatchingDocs.next();
									CurrentIdIterator = CurrentDocs.docIdSet.GetEnumerator();
									if ( CurrentIdIterator != null )
									{
										 DocValuesCache.Clear();
										 CurrentDocValues = CurrentDocs.readDocValues( Field );
									}
							  }
							  else
							  {
									return false;
							  }
						 }
						 return true;
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }

			  internal virtual bool FetchNextEntityId()
			  {
					try
					{
						 if ( EnsureValidDisi() )
						 {
							  int nextDoc = CurrentIdIterator.nextDoc();
							  if ( nextDoc != DocIdSetIterator.NO_MORE_DOCS )
							  {
									Index++;
									Next = CurrentDocValues.get( nextDoc );
									return true;
							  }
							  else
							  {
									CurrentIdIterator = null;
									return FetchNextEntityId();
							  }
						 }
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}

					return false;
			  }
		 }

		 /// <summary>
		 /// Iterates over all per-segment <seealso cref="DocValuesCollector.MatchingDocs"/>. Supports two kinds of lookups.
		 /// One, iterate over all long values of the given field (constructor argument).
		 /// Two, lookup a value for the current doc in a sidecar {@code NumericDocValues} field.
		 /// That is, this iterator has a main field, that drives the iteration and allow for lookups
		 /// in other, secondary fields based on the current document of the main iteration.
		 /// 
		 /// Lookups from this class are not thread-safe. Races can happen when the segment barrier
		 /// is crossed; one thread might think it is reading from one segment while another thread has
		 /// already advanced this Iterator to the next segment, having raced the first thread.
		 /// </summary>
		 public class LongValuesIterator : LongValuesSource, ValuesIterator, PrimitiveLongResourceIterator
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool HasNextConflict;
			  internal bool HasNextDecided;

			  /// <param name="allMatchingDocs"> all <seealso cref="DocValuesCollector.MatchingDocs"/> across all segments </param>
			  /// <param name="totalHits"> the total number of hits across all segments </param>
			  /// <param name="field"> the main field, whose values drive the iteration </param>
			  public LongValuesIterator( IEnumerable<DocValuesCollector.MatchingDocs> allMatchingDocs, int totalHits, string field ) : base( allMatchingDocs, totalHits, field )
			  {
			  }

			  public override long Current()
			  {
					return Next;
			  }

			  public override float CurrentScore()
			  {
					return 0;
			  }

			  public override long GetValue( string field )
			  {
					if ( EnsureValidDisi() )
					{
						 if ( DocValuesCache.ContainsKey( field ) )
						 {
							  return DocValuesCache[field].get( CurrentIdIterator.docID() );
						 }

						 NumericDocValues docValues = CurrentDocs.readDocValues( field );
						 DocValuesCache[field] = docValues;

						 return docValues.get( CurrentIdIterator.docID() );
					}
					else
					{
						 // same as DocValues.emptyNumeric()#get
						 // which means, getValue carries over the semantics of NDV
						 // -1 would also be a possibility here.
						 return 0;
					}
			  }

			  public override bool HasNext()
			  {
					if ( !HasNextDecided )
					{
						 HasNextConflict = FetchNextEntityId();
						 HasNextDecided = true;
					}
					return HasNextConflict;
			  }

			  public override long Next()
			  {
					if ( !HasNext() )
					{
						 throw new NoSuchElementException();
					}
					HasNextDecided = false;
					return Next;
			  }

			  public override int Remaining()
			  {
					return TotalHits - Index;
			  }

			  public override void Close()
			  {
					// nothing to close
			  }
		 }

		 private class LongValuesIndexProgressor : LongValuesSource, IndexProgressor
		 {
			  internal readonly Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor_NodeValueClient Client;

			  internal LongValuesIndexProgressor( IEnumerable<MatchingDocs> allMatchingDocs, int totalHits, string field, Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor_NodeValueClient client ) : base( allMatchingDocs, totalHits, field )
			  {
					this.Client = client;
			  }

			  public override bool Next()
			  {
					while ( FetchNextEntityId() )
					{
						 if ( Client.acceptNode( Next, ( Value[] ) null ) )
						 {
							  return true;
						 }
					}
					return false;
			  }

			  public override void Close()
			  {
					// nothing to close
			  }
		 }

		 /// <summary>
		 /// Holds the documents that were matched per segment.
		 /// </summary>
		 internal sealed class MatchingDocs
		 {

			  /// <summary>
			  /// The {@code LeafReaderContext} for this segment. </summary>
			  public readonly LeafReaderContext Context;

			  /// <summary>
			  /// Which documents were seen. </summary>
			  public readonly DocIdSet DocIdSet;

			  /// <summary>
			  /// Non-sparse scores array. Might be null of no scores were required. </summary>
			  public readonly float[] Scores;

			  /// <summary>
			  /// Total number of hits </summary>
			  public readonly int TotalHits;

			  internal MatchingDocs( LeafReaderContext context, DocIdSet docIdSet, int totalHits, float[] scores )
			  {
					this.Context = context;
					this.DocIdSet = docIdSet;
					this.TotalHits = totalHits;
					this.Scores = scores;
			  }

			  /// <returns> the {@code NumericDocValues} for a given field </returns>
			  /// <exception cref="IllegalArgumentException"> if this field is not indexed with numeric doc values </exception>
			  public NumericDocValues ReadDocValues( string field )
			  {
					try
					{
						 NumericDocValues dv = Context.reader().getNumericDocValues(field);
						 if ( dv == null )
						 {
							  FieldInfo fi = Context.reader().FieldInfos.fieldInfo(field);
							  DocValuesType actual = null;
							  if ( fi != null )
							  {
									actual = fi.DocValuesType;
							  }
							  throw new System.InvalidOperationException( "The field '" + field + "' is not indexed properly, expected NumericDV, but got '" + actual + "'" );
						 }
						 return dv;
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }
		 }

		 /// <summary>
		 /// Used during collection to record matching docs and then return a </summary>
		 /// {<seealso cref= DocIdSet} that contains them. </seealso>
		 private sealed class Docs
		 {
			  internal readonly DocIdSetBuilder Bits;

			  internal Docs( int maxDoc )
			  {
					Bits = new DocIdSetBuilder( maxDoc );
			  }

			  /// <summary>
			  /// Record the given document. </summary>
			  public void AddDoc( int docId )
			  {
					Bits.add( docId );
			  }

			  /// Return the {<seealso cref= DocIdSet} which contains all the recorded docs. </seealso>
			  public DocIdSet DocIdSet
			  {
				  get
				  {
						return Bits.build();
				  }
			  }
		 }

		 private class ReplayingScorer : Scorer
		 {

			  internal readonly float[] Scores;
			  internal int Index;

			  internal ReplayingScorer( float[] scores ) : base( null )
			  {
					this.Scores = scores;
			  }

			  public override float Score()
			  {
					if ( Index < Scores.Length )
					{
						 return Scores[Index++];
					}
					return Float.NaN;
			  }

			  public override int Freq()
			  {
					throw new System.NotSupportedException();
			  }

			  public override DocIdSetIterator Iterator()
			  {
					throw new System.NotSupportedException();
			  }

			  public override int DocID()
			  {
					throw new System.NotSupportedException();
			  }

		 }

		 private sealed class DocsInIndexOrderIterator : AbstractIndexHits<Document>
		 {
			  internal readonly IEnumerator<MatchingDocs> Docs;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int SizeConflict;
			  internal readonly bool KeepScores;
			  internal DocIdSetIterator CurrentIdIterator;
			  internal Scorer CurrentScorer;
			  internal LeafReader CurrentReader;

			  internal DocsInIndexOrderIterator( IList<MatchingDocs> docs, int size, bool keepScores )
			  {
					this.SizeConflict = size;
					this.KeepScores = keepScores;
					this.Docs = docs.GetEnumerator();
			  }

			  public override int Size()
			  {
					return SizeConflict;
			  }

			  public override float CurrentScore()
			  {
					try
					{
						 return CurrentScorer.score();
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }

			  protected internal override Document FetchNextOrNull()
			  {
					if ( EnsureValidDisi() )
					{
						 try
						 {
							  int doc = CurrentIdIterator.nextDoc();
							  if ( doc == DocIdSetIterator.NO_MORE_DOCS )
							  {
									CurrentIdIterator = null;
									CurrentScorer = null;
									CurrentReader = null;
									return FetchNextOrNull();
							  }
							  return CurrentReader.document( doc );
						 }
						 catch ( IOException e )
						 {
							  throw new Exception( e );
						 }
					}
					else
					{
						 return null;
					}
			  }

			  internal bool EnsureValidDisi()
			  {
					while ( CurrentIdIterator == null && Docs.MoveNext() )
					{
						 MatchingDocs matchingDocs = Docs.Current;
						 try
						 {
							  CurrentIdIterator = matchingDocs.DocIdSet.GetEnumerator();
							  if ( KeepScores )
							  {
									CurrentScorer = new ReplayingScorer( matchingDocs.Scores );
							  }
							  else
							  {
									CurrentScorer = new ConstantScoreScorer( null, Float.NaN, CurrentIdIterator );
							  }
							  CurrentReader = matchingDocs.Context.reader();
						 }
						 catch ( IOException e )
						 {
							  throw new Exception( e );
						 }
					}
					return CurrentIdIterator != null;
			  }
		 }

		 private abstract class ScoreDocsIterator : PrefetchingIterator<ScoreDoc>
		 {
			  internal readonly IEnumerator<ScoreDoc> Iterator;
			  internal readonly int[] DocStarts;
			  internal readonly LeafReaderContext[] Contexts;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  protected internal ScoreDoc CurrentDocConflict;

			  internal ScoreDocsIterator( TopDocs docs, LeafReaderContext[] contexts )
			  {
					this.Contexts = contexts;
					this.Iterator = new ArrayIterator<ScoreDoc>( docs.scoreDocs );
					int segments = contexts.Length;
					DocStarts = new int[segments + 1];
					for ( int i = 0; i < segments; i++ )
					{
						 LeafReaderContext context = contexts[i];
						 DocStarts[i] = context.docBase;
					}
					LeafReaderContext lastContext = contexts[segments - 1];
					DocStarts[segments] = lastContext.docBase + lastContext.reader().maxDoc();
			  }

			  public virtual ScoreDoc CurrentDoc
			  {
				  get
				  {
						return CurrentDocConflict;
				  }
			  }

			  protected internal override ScoreDoc FetchNextOrNull()
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( !Iterator.hasNext() )
					{
						 return null;
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					CurrentDocConflict = Iterator.next();
					int subIndex = ReaderUtil.subIndex( CurrentDocConflict.doc, DocStarts );
					LeafReaderContext context = Contexts[subIndex];
					OnNextDoc( CurrentDocConflict.doc - context.docBase, context );
					return CurrentDocConflict;
			  }

			  protected internal abstract void OnNextDoc( int localDocID, LeafReaderContext context );
		 }

		 private sealed class TopDocsIterator : AbstractIndexHits<Document>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int SizeConflict;
			  internal readonly ScoreDocsIterator ScoreDocs;
			  internal Document CurrentDoc;

			  internal TopDocsIterator( TopDocs docs, LeafReaderContext[] contexts )
			  {
					ScoreDocs = new ScoreDocsIteratorAnonymousInnerClass( this, docs, contexts );
					this.SizeConflict = docs.scoreDocs.length;
			  }

			  private class ScoreDocsIteratorAnonymousInnerClass : ScoreDocsIterator
			  {
				  private readonly TopDocsIterator _outerInstance;

				  public ScoreDocsIteratorAnonymousInnerClass( TopDocsIterator outerInstance, TopDocs docs, LeafReaderContext[] contexts ) : base( docs, contexts )
				  {
					  this.outerInstance = outerInstance;
				  }

				  protected internal override void onNextDoc( int localDocID, LeafReaderContext context )
				  {
						outerInstance.UpdateCurrentDocument( localDocID, context.reader() );
				  }
			  }

			  public override int Size()
			  {
					return SizeConflict;
			  }

			  public override float CurrentScore()
			  {
					return ScoreDocs.CurrentDoc.score;
			  }

			  protected internal override Document FetchNextOrNull()
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( !ScoreDocs.hasNext() )
					{
						 return null;
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					ScoreDocs.next();
					return CurrentDoc;
			  }

			  internal void UpdateCurrentDocument( int docID, LeafReader reader )
			  {
					try
					{
						 CurrentDoc = reader.document( docID );
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }
		 }

		 private sealed class TopDocsValuesIterator : ValuesIterator_Adapter
		 {
			  internal readonly ScoreDocsIterator ScoreDocs;
			  internal readonly string Field;
			  internal readonly IDictionary<LeafReaderContext, NumericDocValues> DocValuesCache;
			  internal readonly IDictionary<string, NumericDocValues> ReaderCache;
			  internal long CurrentValue;
			  internal LeafReaderContext CurrentContext;
			  internal int CurrentDocID;

			  internal TopDocsValuesIterator( TopDocs docs, LeafReaderContext[] contexts, string field ) : base( docs.totalHits )
			  {
					this.Field = field;
					DocValuesCache = new Dictionary<LeafReaderContext, NumericDocValues>( contexts.Length );
					ReaderCache = new Dictionary<string, NumericDocValues>();
					ScoreDocs = new ScoreDocsIteratorAnonymousInnerClass( this, docs, contexts );
			  }

			  private class ScoreDocsIteratorAnonymousInnerClass : ScoreDocsIterator
			  {
				  private readonly TopDocsValuesIterator _outerInstance;

				  public ScoreDocsIteratorAnonymousInnerClass( TopDocsValuesIterator outerInstance, TopDocs docs, LeafReaderContext[] contexts ) : base( docs, contexts )
				  {
					  this.outerInstance = outerInstance;
				  }

				  protected internal override void onNextDoc( int localDocID, LeafReaderContext context )
				  {
						_outerInstance.readerCache.Clear();
						_outerInstance.currentContext = context;
						_outerInstance.currentDocID = localDocID;
						outerInstance.LoadNextValue( context, localDocID );
				  }
			  }

			  protected internal override bool FetchNext()
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( ScoreDocs.hasNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 ScoreDocs.next();
						 Index++;
						 return CurrentValue != -1 && Next( CurrentValue );
					}
					return false;
			  }

			  public override long Current()
			  {
					return Index;
			  }

			  public override float CurrentScore()
			  {
					return ScoreDocs.CurrentDoc.score;
			  }

			  public override long GetValue( string field )
			  {
					NumericDocValues fieldValues = ReaderCache.computeIfAbsent( field, this.getDocValues );
					return fieldValues.get( CurrentDocID );
			  }

			  internal NumericDocValues GetDocValues( string field )
			  {
					try
					{
						 return CurrentContext.reader().getNumericDocValues(field);
					}
					catch ( IOException e )
					{
						 throw new Exception( "Fail to read numeric doc values field " + field + " from the document.", e );
					}
			  }

			  internal void LoadNextValue( LeafReaderContext context, int docID )
			  {
					NumericDocValues docValues;
					if ( DocValuesCache.ContainsKey( context ) )
					{
						 docValues = DocValuesCache[context];
					}
					else
					{
						 try
						 {
							  docValues = context.reader().getNumericDocValues(Field);
							  DocValuesCache[context] = docValues;
						 }
						 catch ( IOException e )
						 {
							  throw new Exception( e );
						 }
					}
					if ( docValues != null )
					{
						 CurrentValue = docValues.get( docID );
					}
					else
					{
						 CurrentValue = -1;
					}
			  }
		 }
	}

}