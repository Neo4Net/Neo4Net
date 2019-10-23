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
namespace Neo4Net.Kernel.Impl.Api.index.sampling
{
	using IndexSample = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample;

	/// <summary>
	/// Builds index sample.
	/// It's implementation specific how sample will be build: using index directly or based on samples
	/// provided through various include/exclude calls </summary>
	/// <seealso cref= DefaultNonUniqueIndexSampler </seealso>
	public interface NonUniqueIndexSampler
	{
		 void Include( string value );

		 void Include( string value, long increment );

		 void Exclude( string value );

		 void Exclude( string value, long decrement );

		 IndexSample Result();

		 IndexSample Result( int numDocs );
	}

	 public abstract class NonUniqueIndexSampler_Adapter : NonUniqueIndexSampler
	 {
		 public abstract IndexSample Result( int numDocs );
		 public abstract IndexSample Result();
		  public override void Include( string value )
		  { // no-op
		  }

		  public override void Include( string value, long increment )
		  { // no-op
		  }

		  public override void Exclude( string value )
		  { // no-op
		  }

		  public override void Exclude( string value, long decrement )
		  { // no-op
		  }
	 }

}