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

namespace Neo4Net.Kernel.Api.Internal
{
   using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor;
   using ILabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.ILabelSchemaDescriptor;
   using IRelationTypeSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.IRelationTypeSchemaDescriptor;
   using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;

   /// <summary>
   /// Surface for creating and dropping indexes and constraints.
   /// </summary>
   public interface ISchemaWrite
   {
      /// <summary>
      /// Create index from schema descriptor
      /// </summary>
      /// <param name="descriptor"> description of the index </param>
      /// <returns> the newly created index </returns>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: IndexReference indexCreate(Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
      IIndexReference IndexCreate(SchemaDescriptor descriptor);

      /// <summary>
      /// Create index from schema descriptor
      /// </summary>
      /// <param name="descriptor"> description of the index </param>
      /// <param name="name"> name of the index </param>
      /// <returns> the newly created index </returns>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: IndexReference indexCreate(Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor descriptor, java.util.Optional<String> name) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
      IIndexReference IndexCreate(SchemaDescriptor descriptor, Optional<string> name);

      /// <summary>
      /// Create index from schema descriptor
      /// </summary>
      /// <param name="descriptor"> description of the index </param>
      /// <param name="provider"> name of the desired index provider implementation </param>
      /// <param name="name"> name of the index </param>
      /// <returns> the newly created index </returns>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: IndexReference indexCreate(Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor descriptor, String provider, java.util.Optional<String> name) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
      IIndexReference IndexCreate(SchemaDescriptor descriptor, string provider, Optional<string> name);

      /// <summary>
      /// Drop the given index
      /// </summary>
      /// <param name="index"> the index to drop </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: void indexDrop(IndexReference index) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
      void IndexDrop(IIndexReference index);

      /// <summary>
      /// Create unique property constraint
      /// </summary>
      /// <param name="descriptor"> description of the constraint </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor uniquePropertyConstraintCreate(Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
      ConstraintDescriptor UniquePropertyConstraintCreate(SchemaDescriptor descriptor);

      /// <summary>
      /// Create unique property constraint
      /// </summary>
      /// <param name="descriptor"> description of the constraint </param>
      /// <param name="provider"> name of the desired index provider implementation </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor uniquePropertyConstraintCreate(Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor descriptor, String provider) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
      ConstraintDescriptor UniquePropertyConstraintCreate(SchemaDescriptor descriptor, string provider);

      /// <summary>
      /// Create node key constraint
      /// </summary>
      /// <param name="descriptor"> description of the constraint </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor nodeKeyConstraintCreate(Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
      ConstraintDescriptor NodeKeyConstraintCreate(ILabelSchemaDescriptor descriptor);

      /// <summary>
      /// Create node key constraint
      /// </summary>
      /// <param name="descriptor"> description of the constraint </param>
      /// <param name="provider"> name of the desired index provider implementation </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor nodeKeyConstraintCreate(Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor descriptor, String provider) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
      ConstraintDescriptor NodeKeyConstraintCreate(ILabelSchemaDescriptor descriptor, string provider);

      /// <summary>
      /// Create node property existence constraint
      /// </summary>
      /// <param name="descriptor"> description of the constraint </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor nodePropertyExistenceConstraintCreate(Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
      ConstraintDescriptor NodePropertyExistenceConstraintCreate(ILabelSchemaDescriptor descriptor);

      /// <summary>
      /// Create relationship property existence constraint
      /// </summary>
      /// <param name="descriptor"> description of the constraint </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor relationshipPropertyExistenceConstraintCreate(Neo4Net.Kernel.Api.Internal.Schema.RelationTypeSchemaDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
      ConstraintDescriptor RelationshipPropertyExistenceConstraintCreate(IRelationTypeSchemaDescriptor descriptor);

      /// <summary>
      /// Drop constraint
      /// </summary>
      /// <param name="descriptor"> description of the constraint </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: void constraintDrop(Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
      void ConstraintDrop(ConstraintDescriptor descriptor);
   }
}