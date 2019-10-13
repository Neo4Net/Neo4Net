﻿/*
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
namespace Neo4Net.Storageengine.Api
{

	using ResourceLocker = Neo4Net.Storageengine.Api.@lock.ResourceLocker;
	using ReadableTransactionState = Neo4Net.Storageengine.Api.txstate.ReadableTransactionState;

	/// <summary>
	/// A command representing one unit of change to a <seealso cref="StorageEngine"/>. Commands are created by
	/// <seealso cref="StorageEngine.createCommands(System.Collections.ICollection, ReadableTransactionState, StorageReader, ResourceLocker, long, Function)"/>
	/// and once created can be serialized onto a <seealso cref="WritableChannel"/> and/or passed back to
	/// <seealso cref="StorageEngine.apply(CommandsToApply, TransactionApplicationMode)"/> for application where the
	/// changes represented by the command are actually applied onto storage.
	/// </summary>
	public interface StorageCommand
	{
		 /// <summary>
		 /// Serializes change this command represents into a <seealso cref="WritableChannel"/> for later reading back.
		 /// First byte of command must be type of command.
		 /// </summary>
		 /// <param name="channel"> <seealso cref="WritableChannel"/> to serialize into. </param>
		 /// <exception cref="IOException"> I/O error from channel. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void serialize(WritableChannel channel) throws java.io.IOException;
		 void Serialize( WritableChannel channel );
	}

}