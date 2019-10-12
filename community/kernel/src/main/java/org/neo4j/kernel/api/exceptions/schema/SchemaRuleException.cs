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
namespace Org.Neo4j.Kernel.Api.Exceptions.schema
{
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using SchemaKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaUtil = Org.Neo4j.@internal.Kernel.Api.schema.SchemaUtil;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;

	/// <summary>
	/// Represent something gone wrong related to SchemaRules
	/// </summary>
	internal class SchemaRuleException : SchemaKernelException
	{
		 protected internal readonly SchemaDescriptor Descriptor;
		 protected internal readonly string MessageTemplate;
		 protected internal readonly Org.Neo4j.Storageengine.Api.schema.SchemaRule_Kind Kind;

		 /// <param name="messageTemplate"> Template for String.format. Must match two strings representing the schema kind and the
		 ///                        descriptor </param>
		 protected internal SchemaRuleException( Status status, string messageTemplate, Org.Neo4j.Storageengine.Api.schema.SchemaRule_Kind kind, SchemaDescriptor descriptor ) : base( status, format( messageTemplate, kind.userString().ToLower(), descriptor.UserDescription(SchemaUtil.idTokenNameLookup) ) )
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