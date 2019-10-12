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
namespace Org.Neo4j.Io.pagecache.impl.muninn
{

	/// <summary>
	/// This exception is used to indicate in the stack trace, whether or not
	/// <seealso cref="MuninnPageCursor.usePreciseCursorErrorStackTraces"/> is enabled.
	/// 
	/// This is useful because it allows us to clearly see how much we can trust the stack traces for decoding errors.
	/// </summary>
	internal sealed class CursorExceptionWithPreciseStackTrace : CursorException
	{
		 internal CursorExceptionWithPreciseStackTrace( string message ) : base( message )
		 {
		 }
	}

}