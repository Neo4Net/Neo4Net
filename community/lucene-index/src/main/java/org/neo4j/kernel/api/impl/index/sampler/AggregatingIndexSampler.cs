﻿using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Api.Impl.Index.sampler
{

	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IOUtils = Org.Neo4j.Io.IOUtils;
	using IndexSample = Org.Neo4j.Storageengine.Api.schema.IndexSample;
	using IndexSampler = Org.Neo4j.Storageengine.Api.schema.IndexSampler;

	/// <summary>
	/// Index sampler implementation that provide total sampling result of multiple provided samples, by aggregating their
	/// internal independent samples.
	/// </summary>
	public class AggregatingIndexSampler : IndexSampler
	{
		 private IList<IndexSampler> _indexSamplers;

		 public AggregatingIndexSampler( IList<IndexSampler> indexSamplers )
		 {
			  this._indexSamplers = indexSamplers;
		 }

		 public override IndexSample SampleIndex()
		 {
			  return _indexSamplers.Select( this.sampleIndex ).Aggregate( this.combine ).get();
		 }

		 private IndexSample SampleIndex( IndexSampler sampler )
		 {
			  try
			  {
					return sampler.SampleIndex();
			  }
			  catch ( IndexNotFoundKernelException e )
			  {
					throw new Exception( e );
			  }
		 }

		 public virtual IndexSample Combine( IndexSample sample1, IndexSample sample2 )
		 {
			  long indexSize = Math.addExact( sample1.IndexSize(), sample2.IndexSize() );
			  long uniqueValues = Math.addExact( sample1.UniqueValues(), sample2.UniqueValues() );
			  long sampleSize = Math.addExact( sample1.SampleSize(), sample2.SampleSize() );
			  return new IndexSample( indexSize, uniqueValues, sampleSize );
		 }

		 public override void Close()
		 {
			  IOUtils.closeAllSilently( _indexSamplers );
		 }
	}

}