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
namespace Neo4Net.Kernel.impl.transaction.log
{

	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

	/// <summary>
	/// Provides flush semantics over a <seealso cref="WritableChannel"/>. Essentially, this interface implies the existence of a
	/// buffer over a <seealso cref="WritableChannel"/>, allowing for batching of writes, controlled via the <seealso cref="prepareForFlush"/>
	/// call.
	/// </summary>
	public interface FlushableChannel : WritableChannel, System.IDisposable
	{
		 /// <summary>
		 /// Ensures that all written content will be present in the file channel. This method does not flush, it prepares for
		 /// it, by returning a handle for flushing at a later time.
		 /// The returned handle guarantees that the writes that happened before its creation will be flushed. Implementations
		 /// may choose to flush content that was written after a call to this method was made.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.io.Flushable prepareForFlush() throws java.io.IOException;
		 Flushable PrepareForFlush();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: FlushableChannel put(byte value) throws java.io.IOException;
		 FlushableChannel Put( sbyte value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: FlushableChannel putShort(short value) throws java.io.IOException;
		 FlushableChannel PutShort( short value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: FlushableChannel putInt(int value) throws java.io.IOException;
		 FlushableChannel PutInt( int value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: FlushableChannel putLong(long value) throws java.io.IOException;
		 FlushableChannel PutLong( long value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: FlushableChannel putFloat(float value) throws java.io.IOException;
		 FlushableChannel PutFloat( float value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: FlushableChannel putDouble(double value) throws java.io.IOException;
		 FlushableChannel PutDouble( double value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: FlushableChannel put(byte[] value, int length) throws java.io.IOException;
		 FlushableChannel Put( sbyte[] value, int length );
	}

}