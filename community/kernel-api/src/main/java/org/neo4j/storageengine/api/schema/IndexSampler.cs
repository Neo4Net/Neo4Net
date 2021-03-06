﻿/*
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
namespace Org.Neo4j.Storageengine.Api.schema
{

	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;

	/// <summary>
	/// Component able to sample schema index.
	/// </summary>
	public interface IndexSampler : System.IDisposable
	{

		 /// <summary>
		 /// Sample this index (on the current thread)
		 /// </summary>
		 /// <returns> the index sampling result </returns>
		 /// <exception cref="IndexNotFoundKernelException"> if the index is dropped while sampling </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexSample sampleIndex() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 IndexSample SampleIndex();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void close()
	//	 { // no-op
	//	 }
	}

	public static class IndexSampler_Fields
	{
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 public static readonly IndexSampler Empty = IndexSample::new;
	}

}