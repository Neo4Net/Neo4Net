using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Annotations
{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SupportedSourceVersion(SourceVersion.RELEASE_8) @SupportedAnnotationTypes("org.Neo4Net.helpers.Service.Implementation") public class ServiceProcessor extends AnnotationProcessor
	public class ServiceProcessor : AnnotationProcessor
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override protected void process(javax.lang.model.element.TypeElement annotationType, javax.lang.model.element.Element annotated, javax.lang.model.element.AnnotationMirror annotation, java.util.Map<? extends javax.lang.model.element.ExecutableElement, ? extends javax.lang.model.element.AnnotationValue> values) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 protected internal override void Process<T1>( TypeElement annotationType, Element annotated, AnnotationMirror annotation, IDictionary<T1> values ) where T1 : javax.lang.model.element.ExecutableElement
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (javax.lang.model.element.AnnotationValue o : (java.util.List<? extends javax.lang.model.element.AnnotationValue>) values.values().iterator().next().getValue())
			  foreach ( AnnotationValue o in ( IList<AnnotationValue> ) values.Values.GetEnumerator().next().Value )
			  {
					TypeMirror service = ( TypeMirror ) o.Value;
					AddTo( ( ( TypeElement ) annotated ).QualifiedName.ToString(), "META-INF", "services", service.ToString() );
			  }
		 }
	}

}