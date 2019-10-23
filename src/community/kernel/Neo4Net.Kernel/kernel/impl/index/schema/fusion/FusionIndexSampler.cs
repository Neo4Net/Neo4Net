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
namespace Neo4Net.Kernel.Impl.Index.Schema.fusion
{

	using Exceptions = Neo4Net.Helpers.Exceptions;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException;
	using IndexSample = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample;
	using IndexSampler = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSampler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.asCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.IOUtils.closeAllSilently;

	public class FusionIndexSampler : IndexSampler
	{
		 private readonly IEnumerable<IndexSampler> _samplers;

		 public FusionIndexSampler( IEnumerable<IndexSampler> samplers )
		 {
			  this._samplers = samplers;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample sampleIndex() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
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