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
namespace Org.Neo4j.Kernel.Impl.Index.Schema.fusion
{

	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IndexSample = Org.Neo4j.Storageengine.Api.schema.IndexSample;
	using IndexSampler = Org.Neo4j.Storageengine.Api.schema.IndexSampler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.IOUtils.closeAllSilently;

	public class FusionIndexSampler : IndexSampler
	{
		 private readonly IEnumerable<IndexSampler> _samplers;

		 public FusionIndexSampler( IEnumerable<IndexSampler> samplers )
		 {
			  this._samplers = samplers;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.IndexSample sampleIndex() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override IndexSample SampleIndex()
		 {
			  IList<IndexSample> samples = new List<IndexSample>();
			  Exception exception = null;
			  foreach ( IndexSampler sampler in _samplers )
			  {
					try
					{
						 samples.Add( sampler.SampleIndex() );
					}
					catch ( Exception e ) when ( e is IndexNotFoundKernelException || e is Exception )
					{
						 exception = Exceptions.chain( exception, e );
					}
			  }
			  if ( exception != null )
			  {
					Exceptions.throwIfUnchecked( exception );
					throw ( IndexNotFoundKernelException )exception;
			  }
			  return CombineSamples( samples );
		 }

		 public static IndexSample CombineSamples( IEnumerable<IndexSample> samples )
		 {
			  long indexSize = 0;
			  long uniqueValues = 0;
			  long sampleSize = 0;
			  foreach ( IndexSample sample in samples )
			  {
					indexSize += sample.IndexSize();
					uniqueValues += sample.UniqueValues();
					sampleSize += sample.SampleSize();
			  }
			  return new IndexSample( indexSize, uniqueValues, sampleSize );
		 }

		 public override void Close()
		 {
			  closeAllSilently( asCollection( _samplers ) );
		 }
	}

}