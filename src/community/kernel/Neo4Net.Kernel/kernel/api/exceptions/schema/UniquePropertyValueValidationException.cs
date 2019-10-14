using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Kernel.Api.Exceptions.schema
{

	using Exceptions = Neo4Net.Helpers.Exceptions;
	using TokenNameLookup = Neo4Net.Internal.Kernel.Api.TokenNameLookup;
	using ConstraintValidationException = Neo4Net.Internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexBackedConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.IndexBackedConstraintDescriptor;

	public class UniquePropertyValueValidationException : ConstraintValidationException
	{
		 private readonly ISet<IndexEntryConflictException> _conflicts;

		 public UniquePropertyValueValidationException( IndexBackedConstraintDescriptor constraint, ConstraintValidationException.Phase phase, IndexEntryConflictException conflict ) : this( constraint, phase, Collections.singleton( conflict ) )
		 {
		 }

		 public UniquePropertyValueValidationException( IndexBackedConstraintDescriptor constraint, ConstraintValidationException.Phase phase, ISet<IndexEntryConflictException> conflicts ) : base( constraint, phase, phase == Phase.Verification ? "Existing data" : "New data", BuildCauseChain( conflicts ) )
		 {
			  this._conflicts = conflicts;
		 }

		 private static IndexEntryConflictException BuildCauseChain( ISet<IndexEntryConflictException> conflicts )
		 {
			  IndexEntryConflictException chainedConflicts = null;
			  foreach ( IndexEntryConflictException conflict in conflicts )
			  {
					chainedConflicts = Exceptions.chain( chainedConflicts, conflict );
			  }
			  return chainedConflicts;
		 }

		 public UniquePropertyValueValidationException( IndexBackedConstraintDescriptor constraint, ConstraintValidationException.Phase phase, Exception cause ) : base( constraint, phase, phase == Phase.Verification ? "Existing data" : "New data", cause )
		 {
			  this._conflicts = Collections.emptySet();
		 }

		 public override string GetUserMessage( TokenNameLookup tokenNameLookup )
		 {
			  SchemaDescriptor schema = ConstraintConflict.schema();
			  StringBuilder message = new StringBuilder();
			  for ( IEnumerator<IndexEntryConflictException> iterator = _conflicts.GetEnumerator(); iterator.MoveNext(); )
			  {
					IndexEntryConflictException conflict = iterator.Current;
					message.Append( conflict.EvidenceMessage( tokenNameLookup, schema ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( iterator.hasNext() )
					{
						 message.Append( Environment.NewLine );
					}
			  }
			  return message.ToString();
		 }

		 public virtual ISet<IndexEntryConflictException> Conflicts()
		 {
			  return _conflicts;
		 }
	}

}