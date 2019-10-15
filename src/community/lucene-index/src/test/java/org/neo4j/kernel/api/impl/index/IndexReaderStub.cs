using System;
using System.Text;

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
namespace Neo4Net.Kernel.Api.Impl.Index
{
	using NumberUtils = org.apache.commons.lang3.math.NumberUtils;
	using BinaryDocValues = Org.Apache.Lucene.Index.BinaryDocValues;
	using DocValues = Org.Apache.Lucene.Index.DocValues;
	using DocValuesType = Org.Apache.Lucene.Index.DocValuesType;
	using FieldInfo = Org.Apache.Lucene.Index.FieldInfo;
	using FieldInfos = Org.Apache.Lucene.Index.FieldInfos;
	using Fields = Org.Apache.Lucene.Index.Fields;
	using IndexOptions = Org.Apache.Lucene.Index.IndexOptions;
	using LeafReader = Org.Apache.Lucene.Index.LeafReader;
	using NumericDocValues = Org.Apache.Lucene.Index.NumericDocValues;
	using SortedDocValues = Org.Apache.Lucene.Index.SortedDocValues;
	using SortedNumericDocValues = Org.Apache.Lucene.Index.SortedNumericDocValues;
	using SortedSetDocValues = Org.Apache.Lucene.Index.SortedSetDocValues;
	using StoredFieldVisitor = Org.Apache.Lucene.Index.StoredFieldVisitor;
	using Bits = org.apache.lucene.util.Bits;


	public class IndexReaderStub : LeafReader
	{
		 private Fields _fields;
		 private bool _allDeleted;
		 private string[] _elements = new string[0];
		 private System.Func<string, NumericDocValues> _ndvs = s => DocValues.emptyNumeric();

		 private IOException _throwOnFields;
		 private static FieldInfo _dummyFieldInfo = new FieldInfo( "id", 0, false, true, false, IndexOptions.DOCS, DocValuesType.NONE, -1, Collections.emptyMap() );

		 public IndexReaderStub( Fields fields )
		 {
			  this._fields = fields;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public IndexReaderStub(final org.apache.lucene.index.NumericDocValues ndv)
		 public IndexReaderStub( NumericDocValues ndv )
		 {
			  this._ndvs = s => ndv;
		 }

		 public virtual string[] Elements
		 {
			 set
			 {
				  this._elements = value;
			 }
		 }

		 public override void AddCoreClosedListener( CoreClosedListener listener )
		 {

		 }

		 public override void RemoveCoreClosedListener( CoreClosedListener listener )
		 {

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.lucene.index.Fields fields() throws java.io.IOException
		 public override Fields Fields()
		 {
			  if ( _throwOnFields != null )
			  {
					IOException exception = this._throwOnFields;
					_throwOnFields = null;
					throw exception;
			  }
			  return _fields;
		 }

		 public override NumericDocValues GetNumericDocValues( string field )
		 {
			  return _ndvs.apply( field );
		 }

		 public override BinaryDocValues GetBinaryDocValues( string field )
		 {
			  return DocValues.emptyBinary();
		 }

		 public override SortedDocValues GetSortedDocValues( string field )
		 {
			  return DocValues.emptySorted();
		 }

		 public override SortedNumericDocValues GetSortedNumericDocValues( string field )
		 {
			  return DocValues.emptySortedNumeric( _elements.Length );
		 }

		 public override SortedSetDocValues GetSortedSetDocValues( string field )
		 {
			  return DocValues.emptySortedSet();
		 }

		 public override Bits GetDocsWithField( string field )
		 {
			  throw new Exception( "Not yet implemented." );
		 }

		 public override NumericDocValues GetNormValues( string field )
		 {
			  return DocValues.emptyNumeric();
		 }

		 public override FieldInfos FieldInfos
		 {
			 get
			 {
				  throw new Exception( "Not yet implemented." );
			 }
		 }

		 public override Bits LiveDocs
		 {
			 get
			 {
				  return new BitsAnonymousInnerClass( this );
			 }
		 }

		 private class BitsAnonymousInnerClass : Bits
		 {
			 private readonly IndexReaderStub _outerInstance;

			 public BitsAnonymousInnerClass( IndexReaderStub outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool get( int index )
			 {
				  if ( index >= _outerInstance.elements.Length )
				  {
						throw new System.ArgumentException( "Doc id out of range" );
				  }
				  return !_outerInstance.allDeleted;
			 }

			 public override int length()
			 {
				  return _outerInstance.elements.Length;
			 }
		 }

		 public override void CheckIntegrity()
		 {
		 }

		 public override Fields GetTermVectors( int docID )
		 {
			  throw new Exception( "Not yet implemented." );
		 }

		 public override int NumDocs()
		 {
			  return _allDeleted ? 0 : _elements.Length;
		 }

		 public override int MaxDoc()
		 {
			  return Math.Max( MaxValue(), _elements.Length ) + 1;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void document(int docID, org.apache.lucene.index.StoredFieldVisitor visitor) throws java.io.IOException
		 public override void Document( int docID, StoredFieldVisitor visitor )
		 {
			  visitor.stringField( _dummyFieldInfo, docID.ToString().GetBytes(Encoding.UTF8) );
		 }

		 protected internal override void DoClose()
		 {
		 }

		 private int MaxValue()
		 {
			  return java.util.elements.Select( value => NumberUtils.ToInt( value, 0 ) ).Max().AsInt;
		 }
	}

}