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
	using Analyzer = org.apache.lucene.analysis.Analyzer;


	using SchemaUtil = Neo4Net.Kernel.Api.Internal.Schema.SchemaUtil;
	using Neo4Net.Kernel.Api.Impl.Index;
	using AbstractIndexPartition = Neo4Net.Kernel.Api.Impl.Index.partition.AbstractIndexPartition;
	using IndexPartitionFactory = Neo4Net.Kernel.Api.Impl.Index.partition.IndexPartitionFactory;
	using PartitionSearcher = Neo4Net.Kernel.Api.Impl.Index.partition.PartitionSearcher;
	using PartitionedIndexStorage = Neo4Net.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;
	using TokenHolder = Neo4Net.Kernel.impl.core.TokenHolder;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;

	public class LuceneFulltextIndex : AbstractLuceneIndex<FulltextIndexReader>, System.IDisposable
	{
		 private readonly Analyzer _analyzer;
		 private readonly string _identifier;
		 private readonly EntityType _type;
		 private readonly ICollection<string> _properties;
		 private readonly TokenHolder _propertyKeyTokenHolder;
		 private readonly File _transactionsFolder;

		 internal LuceneFulltextIndex( PartitionedIndexStorage storage, IndexPartitionFactory partitionFactory, FulltextIndexDescriptor descriptor, TokenHolder propertyKeyTokenHolder ) : base( storage, partitionFactory, descriptor )
		 {
			  this._analyzer = descriptor.Analyzer();
			  this._identifier = descriptor.Name;
			  this._type = descriptor.Schema().entityType();
			  this._properties = descriptor.PropertyNames();
			  this._propertyKeyTokenHolder = propertyKeyTokenHolder;
			  File indexFolder = storage.IndexFolder;
			  _transactionsFolder = new File( indexFolder.Parent, indexFolder.Name + ".tx" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void open() throws java.io.IOException
		 public override void Open()
		 {
			  base.Open();
			  IndexStorage.prepareFolder( _transactionsFolder );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  base.Close();
			  IndexStorage.cleanupFolder( _transactionsFolder );
		 }

		 public override string ToString()
		 {
			  return "LuceneFulltextIndex{" +
						"analyzer=" + _analyzer.GetType().Name +
						", identifier='" + _identifier + '\'' +
						", type=" + _type +
						", properties=" + _properties +
						", descriptor=" + DescriptorConflict.userDescription( SchemaUtil.idTokenNameLookup ) +
						'}';
		 }

		 internal virtual string[] PropertiesArray
		 {
			 get
			 {
				  return _properties.toArray( new string[0] );
			 }
		 }

		 internal virtual Analyzer Analyzer
		 {
			 get
			 {
				  return _analyzer;
			 }
		 }

		 internal virtual TokenHolder PropertyKeyTokenHolder
		 {
			 get
			 {
				  return _propertyKeyTokenHolder;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected FulltextIndexReader createSimpleReader(java.util.List<Neo4Net.kernel.api.impl.index.partition.AbstractIndexPartition> partitions) throws java.io.IOException
		 protected internal override FulltextIndexReader CreateSimpleReader( IList<AbstractIndexPartition> partitions )
		 {
			  AbstractIndexPartition singlePartition = GetFirstPartition( partitions );
			  SearcherReference searcher = new PartitionSearcherReference( singlePartition.AcquireSearcher() );
			  return new SimpleFulltextIndexReader( searcher, PropertiesArray, _analyzer, _propertyKeyTokenHolder );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected FulltextIndexReader createPartitionedReader(java.util.List<Neo4Net.kernel.api.impl.index.partition.AbstractIndexPartition> partitions) throws java.io.IOException
		 protected internal override FulltextIndexReader CreatePartitionedReader( IList<AbstractIndexPartition> partitions )
		 {
			  IList<PartitionSearcher> searchers = AcquireSearchers( partitions );
			  return new PartitionedFulltextIndexReader( searchers, PropertiesArray, _analyzer, _propertyKeyTokenHolder );
		 }
	}

}