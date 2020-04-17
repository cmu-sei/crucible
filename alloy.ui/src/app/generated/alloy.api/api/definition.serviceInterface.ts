/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/
/**
 * Alloy API
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: v1
 * 
 *
 * NOTE: This class is auto generated by the swagger code generator program.
 * https://github.com/swagger-api/swagger-codegen.git
 * Do not edit the class manually.
 */
import { HttpHeaders }                                       from '@angular/common/http';

import { Observable }                                        from 'rxjs';

import { ApiError } from '../model/apiError';
import { Definition } from '../model/definition';


import { Configuration }                                     from '../configuration';


export interface DefinitionServiceInterface {
    defaultHeaders: HttpHeaders;
    configuration: Configuration;
    

    /**
    * Creates a new Definition
    * Creates a new Definition with the attributes specified  &lt;para /&gt;  Accessible only to a SuperUser or an Administrator
    * @param definition The data to create the Definition with
    */
    createDefinition(definition?: Definition, extraHttpRequestParams?: any): Observable<Definition>;

    /**
    * Deletes an Definition
    * Deletes an Definition with the specified id  &lt;para /&gt;  Accessible only to a SuperUser or a User on an Admin Team within the specified Definition
    * @param id The id of the Definition to delete
    */
    deleteDefinition(id: string, extraHttpRequestParams?: any): Observable<{}>;

    /**
    * Gets a specific Definition by id
    * Returns the Definition with the id specified  &lt;para /&gt;  Accessible to a SuperUser or a User that is a member of a Team within the specified Definition
    * @param id The id of the Definition
    */
    getDefinition(id: string, extraHttpRequestParams?: any): Observable<Definition>;

    /**
    * Gets all Definition in the system
    * Returns a list of all of the Definitions in the system.  &lt;para /&gt;  Only accessible to a SuperUser
    */
    getDefinitions(extraHttpRequestParams?: any): Observable<Array<Definition>>;

    /**
    * Updates an Definition
    * Updates an Definition with the attributes specified  &lt;para /&gt;  Accessible only to a SuperUser or a User on an Admin Team within the specified Definition
    * @param id The Id of the Exericse to update
    * @param definition The updated Definition values
    */
    updateDefinition(id: string, definition?: Definition, extraHttpRequestParams?: any): Observable<Definition>;

}
