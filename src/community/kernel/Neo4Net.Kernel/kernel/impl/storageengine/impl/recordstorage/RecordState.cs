﻿using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{

	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;

	/// <summary>
	/// Keeper of state that is about to be committed. That state can be <seealso cref="extractCommands(System.Collections.ICollection) extracted"/>
	/// into a list of <seealso cref="Command commands"/>.
	/// </summary>
	public interface RecordState
	{
		 /// <returns> whether or not there are any changes in here. If {@code true} then <seealso cref="Command commands"/>
		 /// can be <seealso cref="extractCommands(System.Collections.ICollection) extracted"/>. </returns>
		 bool HasChanges();

		 /// <summary>
		 /// Extracts this record state in the form of <seealso cref="Command commands"/> into the supplied {@code target} list. </summary>
		 /// <param name="target"> list that commands will be added into. </param>
		 /// <exception cref="TransactionFailureException"> if the state is invalid or not applicable. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void extractCommands(java.util.Collection<Neo4Net.Kernel.Api.StorageEngine.StorageCommand> target) throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
		 void ExtractCommands( ICollection<StorageCommand> target );
	}

}