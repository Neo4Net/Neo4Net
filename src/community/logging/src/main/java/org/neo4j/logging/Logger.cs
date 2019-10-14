using System;

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
namespace Neo4Net.Logging
{

	/// <summary>
	/// A log into which messages can be written
	/// </summary>
	public interface Logger
	{
		 /// <param name="message"> The message to be written </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void log(@Nonnull String message);
		 void Log( string message );

		 /// <param name="message">   The message to be written </param>
		 /// <param name="throwable"> An exception that will also be written </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void log(@Nonnull String message, @Nonnull Throwable throwable);
		 void Log( string message, Exception throwable );

		 /// <param name="format">    A string format for writing a message </param>
		 /// <param name="arguments"> Arguments to substitute into the message according to the {@code format} </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void log(@Nonnull String format, @Nullable Object... arguments);
		 void Log( string format, params object[] arguments );

		 /// <summary>
		 /// Used to temporarily write several messages in bulk. The implementation may choose to
		 /// disable flushing, and may also block other operations until the bulk update is completed.
		 /// </summary>
		 /// <param name="consumer"> A callback operation that accepts an equivalent <seealso cref="Logger"/> </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void bulk(@Nonnull Consumer<Logger> consumer);
		 void Bulk( Consumer<Logger> consumer );
	}

}