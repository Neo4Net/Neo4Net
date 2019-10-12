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
namespace Neo4Net.Kernel.Api.Exceptions.schema
{
	using TokenNameLookup = Neo4Net.@internal.Kernel.Api.TokenNameLookup;
	using SchemaKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaUtil = Neo4Net.@internal.Kernel.Api.schema.SchemaUtil;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;

	/// <summary>
	/// Represent something gone wrong related to SchemaRules
	/// </summary>
	internal class SchemaRuleException : SchemaKernelException
	{
		 protected internal readonly SchemaDescriptor Descriptor;
		 protected internal readonly string MessageTemplate;
		 protected internal readonly Neo4Net.Storageengine.Api.schema.SchemaRule_Kind Kind;

		 /// <param name="messageTemplate"> Template for String.format. Must match two strings representing the schema kind and the
		 ///                        descriptor </param>
		 protected internal SchemaRuleException( Status status, string messageTemplate, Neo4Net.Storageengine.Api.schema.SchemaRule_Kind kind, SchemaDescriptor descriptor ) : base( status, format( messageTemplate, kind.userString().ToLower(), descriptor.UserDescription(SchemaUtil.idTokenNameLookup) ) )
		 {
			  this.Descriptor = descriptor;
			  this.MessageTemplate = messageTemplate;
			  this.Kind = kind;
		 }

		 public override string GetUserMessage( TokenNameLookup tokenNameLookup )
		 {
			  return format( MessageTemplate, Kind.userString().ToLower(), Descriptor.userDescription(tokenNameLookup) );
		 }
	}

}